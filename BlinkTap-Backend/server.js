const express = require('express');
const cors = require('cors');
const admin = require('./firebase');

const app = express();
const port = 3000;

app.use(cors());
app.use(express.json());

// ✅ Firebase client SDK for login
const { initializeApp } = require('firebase/app');
const { getAuth, signInWithEmailAndPassword } = require('firebase/auth');

const firebaseConfig = {
  apiKey: "AIzaSyAXJ_AjyzDuob0o2PBvHUULGVSBwJ9wn1M",
  authDomain: "blinktap-50fa0.firebaseapp.com",
};

const firebaseApp = initializeApp(firebaseConfig);
const auth = getAuth(firebaseApp);

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

app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
