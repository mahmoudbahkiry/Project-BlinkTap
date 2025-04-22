const http = require('http');
const https = require('https');

// Test function to make a direct HTTP request to the profile endpoint
async function testProfileAPI() {
  // Test data
  const testEmail = 'test-direct@example.com';
  const testProfession = 'Table Tennis Players';
  
  console.log('Testing direct API call to /profile endpoint...');
  
  // Make a POST request to create or update a profile
  await makeRequest('POST', '/profile', {
    email: testEmail,
    profession: testProfession
  });
  
  // Make a GET request to retrieve the profile
  await makeRequest('GET', `/profile?email=${encodeURIComponent(testEmail)}`);
  
  console.log('API test completed!');
}

// Helper function to make HTTP requests
function makeRequest(method, path, data = null) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 3000,
      path: path,
      method: method,
      headers: {
        'Content-Type': 'application/json'
      }
    };
    
    console.log(`Making ${method} request to: http://localhost:3000${path}`);
    
    if (data) {
      console.log('Request body:', data);
    }
    
    const req = http.request(options, (res) => {
      console.log(`Response status: ${res.statusCode}`);
      console.log('Response headers:', res.headers);
      
      let responseData = '';
      
      res.on('data', (chunk) => {
        responseData += chunk;
      });
      
      res.on('end', () => {
        console.log('Response body:', responseData);
        
        try {
          const jsonResponse = JSON.parse(responseData);
          console.log('Parsed JSON response:', jsonResponse);
          resolve(jsonResponse);
        } catch (error) {
          console.log('Response is not valid JSON:', responseData);
          resolve(responseData);
        }
      });
    });
    
    req.on('error', (error) => {
      console.error(`Request error: ${error.message}`);
      reject(error);
    });
    
    if (data) {
      req.write(JSON.stringify(data));
    }
    
    req.end();
  });
}

// Run the test
testProfileAPI()
  .then(() => {
    console.log('Test completed successfully!');
    process.exit(0);
  })
  .catch((error) => {
    console.error('Test failed:', error);
    process.exit(1);
  }); 