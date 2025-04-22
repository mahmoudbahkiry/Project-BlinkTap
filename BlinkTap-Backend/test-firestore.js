const admin = require('./firebase');
const db = admin.firestore();

// Test function to verify Firestore connection and collection creation
async function testFirestore() {
  try {
    console.log('Testing Firestore connection...');
    
    // Check if the professions collection exists
    const collections = await db.listCollections();
    const collectionIds = collections.map(collection => collection.id);
    
    console.log('Existing collections:', collectionIds);
    
    // Test data
    const testEmail = 'test@example.com';
    const testProfession = 'Boxers';
    
    // Query for existing profile
    const professionsRef = db.collection('professions');
    const snapshot = await professionsRef.where('email', '==', testEmail).get();
    
    if (snapshot.empty) {
      console.log(`No existing profile found for ${testEmail}, creating new one...`);
      
      // Create new profile
      const docRef = await professionsRef.add({
        email: testEmail,
        profession: testProfession,
        createdAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Created new profile with ID: ${docRef.id}`);
    } else {
      // Update existing profile
      const docId = snapshot.docs[0].id;
      const currentProfession = snapshot.docs[0].data().profession;
      
      console.log(`Found existing profile for ${testEmail} with profession: ${currentProfession}`);
      
      // Update to a different profession
      const newProfession = currentProfession === 'Boxers' ? 'Tennis Players' : 'Boxers';
      
      await professionsRef.doc(docId).update({
        profession: newProfession,
        updatedAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Updated profile profession to: ${newProfession}`);
    }
    
    // Verify the data was saved
    const updatedSnapshot = await professionsRef.where('email', '==', testEmail).get();
    
    if (!updatedSnapshot.empty) {
      const userData = updatedSnapshot.docs[0].data();
      console.log('Retrieved profile data:', userData);
    }
    
    console.log('Firestore test completed successfully!');
  } catch (error) {
    console.error('Error testing Firestore:', error);
  } finally {
    // Exit process when done
    process.exit(0);
  }
}

// Run the test
testFirestore(); 