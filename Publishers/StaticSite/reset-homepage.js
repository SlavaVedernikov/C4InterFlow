const fs = require('fs');
const path = require('path');

// Read the package.json file
const packageJsonPath = path.resolve(__dirname, 'package.json');
const packageJson = require(packageJsonPath);

// Remove the "homepage" field
delete packageJson.homepage;

// Write the package.json file back to disk
fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2));