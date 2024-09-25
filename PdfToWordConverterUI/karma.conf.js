module.exports = function (config) {
    config.set({
      // Other Karma configuration settings
  
      autoWatch: false,  // Disable automatic re-running of tests on file changes
      singleRun: true,   // Run tests once and then exit
      browsers: ['Chrome'], // Specify the browser you want to run (e.g., Chrome)
  
      // Set the number of browser instances to run tests
      concurrency: 1,   // Only allow 1 browser to run at a time to avoid multiple browser launches
    });
  };
  