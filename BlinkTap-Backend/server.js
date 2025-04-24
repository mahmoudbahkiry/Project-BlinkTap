const express = require('express');
const cors = require('cors');
const admin = require('./firebase');

const app = express();
const port = 3000;

// Enable CORS for all routes
app.use(cors({
  origin: '*', // Allow all origins
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization']
}));

// Parse JSON bodies
app.use(express.json());

const { initializeApp } = require('firebase/app');
const { getAuth, signInWithEmailAndPassword } = require('firebase/auth');

const firebaseConfig = {
  apiKey: "AIzaSyAXJ_AjyzDuob0o2PBvHUULGVSBwJ9wn1M",
  authDomain: "blinktap-50fa0.firebaseapp.com",
};

const firebaseApp = initializeApp(firebaseConfig);
const auth = getAuth(firebaseApp);

// Get Firestore instance
const db = admin.firestore();

// Initialize professions collection if it doesn't exist (just a check)
const initializeFirestore = async () => {
  try {
    const collections = await db.listCollections();
    const collectionIds = collections.map(collection => collection.id);
    
    if (!collectionIds.includes('professions')) {
      console.log('Initializing professions collection');
      // The collection will be created when the first document is added
    } else {
      console.log('Professions collection already exists');
    }
  } catch (error) {
    console.error('Error checking collections:', error);
  }
};

// Run initialization on server start
initializeFirestore();

// Log all incoming requests
app.use((req, res, next) => {
  console.log(`[${new Date().toISOString()}] ${req.method} ${req.url}`);
  if (req.method === 'POST' || req.method === 'PUT') {
    console.log('Request body:', req.body);
  } else if (req.method === 'GET') {
    console.log('Query params:', req.query);
  }
  next();
});

app.post('/register', async (req, res) => {
  const { email, password } = req.body;
  try {
    const user = await admin.auth().createUser({ email, password });
    res.status(200).json({ message: 'User registered', uid: user.uid });
  } catch (error) {
    res.status(400).json({ error: error.message });
  }
});

app.post('/login', async (req, res) => {
  const { email, password } = req.body;
  try {
    const userCredential = await signInWithEmailAndPassword(auth, email, password);
    const token = await userCredential.user.getIdToken();
    res.status(200).json({ message: 'Login successful', token });
  } catch (error) {
    res.status(401).json({ error: error.message });
  }
});

// Get user profile data
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

// Update or create user profile data
app.post('/profile', async (req, res) => {
  console.log('Received POST /profile request');
  console.log('Headers:', req.headers);
  console.log('Body:', req.body);
  
  // Check if the body is properly parsed
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
  
  console.log(`POST /profile - Received profile update: ${email}, profession: ${profession}`);
  
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

// Add a DEBUG endpoint to check if the server is running
app.get('/debug', (req, res) => {
  console.log('DEBUG endpoint called');
  res.status(200).json({ 
    status: 'ok',
    message: 'Server is running',
    timestamp: new Date().toISOString()
  });
});

// Add a simple echo endpoint for testing
app.post('/debug/echo', (req, res) => {
  console.log('ECHO endpoint called');
  console.log('Request headers:', req.headers);
  console.log('Request body:', req.body);
  
  // Echo back the request data
  res.status(200).json({
    status: 'ok',
    message: 'Echo response',
    timestamp: new Date().toISOString(),
    receivedHeaders: req.headers,
    receivedBody: req.body
  });
});

// New endpoint to handle score uploads
app.post('/scores', async (req, res) => {
  console.log('Received POST /scores request');
  console.log('Headers:', req.headers);
  console.log('Body:', req.body);
  
  if (!req.body || typeof req.body !== 'object') {
    console.error('Invalid request body format. Received:', req.body);
    return res.status(400).json({ error: 'Invalid request body format' });
  }
  
  const { email, averageReactionTime, timestamp } = req.body;
  
  if (!email) {
    console.error('POST /scores - Email is required but was not provided');
    return res.status(400).json({ error: 'Email is required' });
  }
  
  if (averageReactionTime === undefined) {
    console.error(`POST /scores - Average reaction time is required but was not provided for email: ${email}`);
    return res.status(400).json({ error: 'Average reaction time is required' });
  }
  
  console.log(`POST /scores - Received score: ${email}, average reaction time: ${averageReactionTime}ms, timestamp: ${timestamp}`);
  
  try {
    // Get a reference to the scores collection and the user's document
    const scoresRef = db.collection('scores');
    const userScoreRef = scoresRef.doc(email);
    
    // Get the current document or create it if it doesn't exist
    const doc = await userScoreRef.get();
    
    if (!doc.exists) {
      // Create a new document for this user
      console.log(`Creating new scores document for user: ${email}`);
      
      await userScoreRef.set({
        email: email,
        testResults: [{
          averageReactionTime: averageReactionTime,
          timestamp: timestamp || admin.firestore.FieldValue.serverTimestamp()
        }],
        createdAt: admin.firestore.FieldValue.serverTimestamp(),
        updatedAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Created new scores document for email: ${email}`);
      return res.status(200).json({ message: 'Score recorded successfully' });
    } else {
      // Update existing document by adding new test result
      console.log(`Updating existing scores document for user: ${email}`);
      
      // Get existing data
      const userData = doc.data();
      const testResults = userData.testResults || [];
      
      // Add new test result
      testResults.push({
        averageReactionTime: averageReactionTime,
        timestamp: timestamp || admin.firestore.FieldValue.serverTimestamp()
      });
      
      // Update the document
      await userScoreRef.update({
        testResults: testResults,
        updatedAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Updated scores for ${email}, total test results: ${testResults.length}`);
      return res.status(200).json({ message: 'Score recorded successfully' });
    }
  } catch (error) {
    console.error('Error recording score:', error);
    console.error(error.stack);
    return res.status(500).json({ error: 'Failed to record score', details: error.message });
  }
});

// New endpoint to get the best (lowest) score for a user
app.get('/best-score', async (req, res) => {
  const { email } = req.query;
  
  if (!email) {
    console.error('GET /best-score - Email is required but was not provided');
    return res.status(400).json({ error: 'Email is required' });
  }
  
  console.log(`GET /best-score - Looking up best score for email: ${email}`);
  
  try {
    // Get a reference to the scores collection and the user's document
    const scoresRef = db.collection('scores');
    const userScoreRef = scoresRef.doc(email);
    
    // Get the current document
    const doc = await userScoreRef.get();
    
    if (!doc.exists) {
      // No scores found for this user
      console.log(`No scores found for email: ${email}`);
      return res.status(200).json({ bestScore: 0 });
    }
    
    // Get user data and test results
    const userData = doc.data();
    const testResults = userData.testResults || [];
    
    if (testResults.length === 0) {
      console.log(`No test results found for email: ${email}`);
      return res.status(200).json({ bestScore: 0 });
    }
    
    // Find the lowest averageReactionTime
    let bestScore = Number.MAX_VALUE;
    for (const result of testResults) {
      if (result.averageReactionTime < bestScore) {
        bestScore = result.averageReactionTime;
      }
    }
    
    console.log(`Best score for ${email}: ${bestScore}ms`);
    return res.status(200).json({ bestScore: Math.round(bestScore) });
  } catch (error) {
    console.error('Error getting best score:', error);
    console.error(error.stack);
    return res.status(500).json({ error: 'Failed to get best score', details: error.message });
  }
});

// Start the server
app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});