module.exports = {
  testMatch: [
    "**/tests/unit/**/*.test.js",
    "**/tests/unit/**/*.spec.js"
  ],
  testPathIgnorePatterns: [
    "/node_modules/"
  ],
  clearMocks: true,
  collectCoverage: true,
  coverageDirectory: "coverage",
  coveragePathIgnorePatterns: [
    "/node_modules/",
    "/tests/"
  ]
};
