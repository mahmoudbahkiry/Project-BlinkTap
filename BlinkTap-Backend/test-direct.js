const admin = require('./firebase');
const db = admin.firestore();

// The email to use for testing
const testEmail = 'mahmoud@gmail.com';
const testProfession = 'Tennis Players';

async function testDirectFirestore() {
  try {
    console.log('Testing direct Firestore operations...');
    
    // Check if the professions collection exists
    const collections = await db.listCollections();
    const collectionIds = collections.map(collection => collection.id);
    console.log('Available collections:', collectionIds);
    
    // Query for existing profile with the given email
    const professionsRef = db.collection('professions');
    const snapshot = await professionsRef.where('email', '==', testEmail).get();
    
    if (snapshot.empty) {
      console.log(`No existing profile found for email: ${testEmail}`);
      console.log(`Creating new profile with profession: ${testProfession}`);
      
      // Create a new profile
      const docRef = await professionsRef.add({
        email: testEmail,
        profession: testProfession,
        createdAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log(`Created profile with ID: ${docRef.id}`);
    } else {
      const doc = snapshot.docs[0];
      const existingData = doc.data();
      
      console.log(`Found existing profile for ${testEmail}:`);
      console.log(existingData);
      
      console.log(`Updating profession to: ${testProfession}`);
      
      // Update the existing profile
      await doc.ref.update({
        profession: testProfession,
        updatedAt: admin.firestore.FieldValue.serverTimestamp()
      });
      
      console.log('Profile updated successfully');
    }
    
    // Verify the updated profile
    const updatedSnapshot = await professionsRef.where('email', '==', testEmail).get();
    
    if (!updatedSnapshot.empty) {
      const updatedDoc = updatedSnapshot.docs[0];
      const updatedData = updatedDoc.data();
      
      console.log('Current profile data:');
      console.log({
        id: updatedDoc.id,
        ...updatedData
      });
      
      if (updatedData.profession === testProfession) {
        console.log('Test successful! Profession was saved correctly.');
      } else {
        console.error(`Test failed! Expected profession "${testProfession}" but got "${updatedData.profession}"`);
      }
    } else {
      console.error('Test failed! Could not find the profile after update.');
    }
    
  } catch (error) {
    console.error('Error during test:', error);
  } finally {
    process.exit(0);
  }
}

// Run the test
testDirectFirestore(); 