const express = require('express');
const cors = require('cors');
const admin = require('./firebase');

// Create a test server
const app = express();
const port = 3000; // Same port as the main server

// Enable CORS for all routes
app.use(cors({
  origin: '*',
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization']
}));

// Parse JSON bodies
app.use(express.json());

// Get Firestore instance
const db = admin.firestore();

// Log all incoming requests for debugging
app.use((req, res, next) => {
  console.log(`[${new Date().toISOString()}] ${req.method} ${req.url}`);
  if (req.method === 'POST' || req.method === 'PUT') {
    console.log('Request body:', JSON.stringify(req.body, null, 2));
  } else if (req.method === 'GET') {
    console.log('Query params:', req.query);
  }
  next();
});

// Debug endpoint to test server is running
app.get('/debug', (req, res) => {
  console.log('Debug endpoint called');
  res.status(200).json({
    status: 'ok',
    message: 'Test server is running',
    timestamp: new Date().toISOString()
  });
});

// Get profile endpoint
app.get('/profile', async (req, res) => {
  const { email } = req.query;
  
  if (!email) {
    console.error('GET /profile - Email is required but was not provided');
    return res.status(400).json({ error: 'Email is required' });
  }
  
  console.log(`GET /profile - Looking up profile for email: ${email}`);
  
  try {
    // Query Firestore to find user profile by email
    const professionsRef = db.collection('professions');
    const snapshot = await professionsRef.where('email', '==', email).get();
    
    if (snapshot.empty) {
      // No profile found, return empty data
      console.log(`No profession found for email: ${email}`);
      return res.status(200).json({ email: email, profession: '' });
    }
    
    // Return the first matching document (there should only be one)
    const userData = snapshot.docs[0].data();
    console.log(`Found profession for ${email}: ${userData.profession}`);
    res.status(200).json({ email: userData.email, profession: userData.profession || '' });
  } catch (error) {
    console.error('Error getting profile:', error);
    res.status(500).json({ error: 'Failed to get profile' });
  }
});

// Update or create profile endpoint
app.post('/profile', async (req, res) => {
  console.log('Received POST /profile request');
  console.log('Headers:', req.headers);
  console.log('Body:', req.body);
  
  if (!req.body || typeof req.body !== 'object') {
    console.error('Invalid request body format. Received:', req.body);
    return res.status(400).json({ error: 'Invalid request body format' });
  }
  
  const { email, profession } = req.body;
  
  if (!email) {
    console.error('POST /profile - Email is required but was not provided');
    return res.status(400).json({ error: 'Email is required' });
  }
  
  if (!profession) {
    console.error(`POST /profile - Profession is required but was not provided for email: ${email}`);
    return res.status(400).json({ error: 'Profession is required' });
  }
  
  console.log(`POST /profile - Received profile update for ${email}: profession=${profession}`);
  
  try {
    // Check if user profile already exists
    const professionsRef = db.collection('professions');
    console.log(`Querying Firestore for email: ${email}`);
    const snapshot = await professionsRef.where('email', '==', email).get();
    
    let result;
    
    if (snapshot.empty) {
      // Create new user profile
      console.log(`Creating new profession record for email: ${email}`);
      const docRef = await professionsRef.add({
        email: email,
        profession: profession,
        createdAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Created new profession record with ID: ${docRef.id}`);
      result = { message: 'Profile created successfully', id: docRef.id };
    } else {
      // Update existing profile
      const userDoc = snapshot.docs[0];
      console.log(`Updating existing profession record with ID: ${userDoc.id}`);
      await userDoc.ref.update({
        profession: profession,
        updatedAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Updated profession for ${email} to: ${profession}`);
      result = { message: 'Profile updated successfully', id: userDoc.id };
    }
    
    // Verify the update was successful by fetching the data again
    const verifySnapshot = await professionsRef.where('email', '==', email).get();
    if (!verifySnapshot.empty) {
      const userData = verifySnapshot.docs[0].data();
      console.log('Verified profile data after update:', userData);
    }
    
    return res.status(200).json(result);
  } catch (error) {
    console.error('Error updating profile:', error);
    console.error(error.stack);
    res.status(500).json({ error: 'Failed to update profile', details: error.message });
  }
});

// Start the server
app.listen(port, () => {
  console.log(`Test server running at http://localhost:${port}`);
  console.log('=== TEST SERVER FOR DEBUGGING ONLY ===');
  console.log('This server can verify if the Unity client can successfully save professions.');
  console.log('To test:');
  console.log('1. Run this server using: node test-server.js');
  console.log('2. Open your Unity project');
  console.log('3. Log in with your account and go to the profile panel');
  console.log('4. Select a profession and click Save');
  console.log('5. Check this console to see if the request was received and processed');
}); 