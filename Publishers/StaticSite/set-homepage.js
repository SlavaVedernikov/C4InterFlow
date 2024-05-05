const fs = require('fs');
const path = require('path');

// Read the package.json file
const packageJsonPath = path.resolve(__dirname, 'package.json');
const packageJson = require(packageJsonPath);

// Set the "homepage" field to the HOMEPAGE environment variable
packageJson.homepage = process.env.HOMEPAGE;

// Write the package.json file back to disk
fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2));