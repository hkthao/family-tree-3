Run npm run test:coverage --prefix frontend
> frontend@0.0.0 test:coverage
> vitest run --coverage --coverage.threshold.statements=80 --coverage.threshold.branches=80 --coverage.threshold.functions=80 --coverage.threshold.lines=80
node:internal/modules/esm/resolve:283
    throw new ERR_MODULE_NOT_FOUND(
          ^
Error [ERR_MODULE_NOT_FOUND]: Cannot find module '/home/runner/work/family-tree-3/family-tree-3/frontend/node_modules/vitest/dist/cli.js' imported from /home/runner/work/family-tree-3/family-tree-3/frontend/node_modules/vitest/vitest.mjs
    at finalizeResolution (node:internal/modules/esm/resolve:283:11)
    at moduleResolve (node:internal/modules/esm/resolve:952:10)
    at defaultResolve (node:internal/modules/esm/resolve:1188:11)
    at ModuleLoader.defaultResolve (node:internal/modules/esm/loader:708:12)
    at #cachedDefaultResolve (node:internal/modules/esm/loader:657:25)
    at ModuleLoader.resolve (node:internal/modules/esm/loader:640:38)
    at ModuleLoader.getModuleJobForImport (node:internal/modules/esm/loader:264:38)
    at ModuleJob._link (node:internal/modules/esm/module_job:168:49) {
  code: 'ERR_MODULE_NOT_FOUND',
  url: 'file:///home/runner/work/family-tree-3/family-tree-3/frontend/node_modules/vitest/dist/cli.js'
}
Node.js v20.19.5
Error: Process completed with exit code 1.