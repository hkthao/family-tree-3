module.exports = {
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  testEnvironment: 'node',
  // You might need to add other Jest configurations here if present in package.json
  // For example:
  // clearMocks: true,
  // coverageDirectory: "coverage",
};
