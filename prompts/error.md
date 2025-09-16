The failing job is caused by TypeScript errors during the build step. Here are the main issues and how to fix each:

---

### 1. Invalid `--lib` Argument in TypeScript Config

**Error:**
```
error TS6046: Argument for '--lib' option must be: 'es5', 'es6', 'es2015', ...
```
**Solution:**  
Check your `tsconfig.json` and any referenced configs (e.g., `node_modules/@tsconfig/node22/tsconfig.json`).  
Make sure the `lib` array only contains valid values (e.g., `"es2022"`, `"dom"`).  
**Example:**
```json
"lib": ["es2022", "dom"]
```
Remove any invalid or misspelled options.

---

### 2. `tsBuildInfoFile` Requires `incremental` or `composite`

**Error:**
```
error TS5069: Option 'tsBuildInfoFile' cannot be specified without specifying option 'incremental' or option 'composite'.
```
**Solution:**  
In all `tsconfig.*.json` files (e.g., `tsconfig.node.json`, `tsconfig.app.json`, `tsconfig.vitest.json`),  
either remove the `tsBuildInfoFile` entry, or add `"incremental": true` or `"composite": true`.
**Example:**
```json
"incremental": true,
"tsBuildInfoFile": "./.tsbuildinfo"
```
or simply delete `"tsBuildInfoFile"` if not needed.

---

### 3. Type Errors in `@vitejs/plugin-vue` Declarations

**Error:**
```
node_modules/@vitejs/plugin-vue/dist/index.d.ts(120,...): error TS1003: Identifier expected.
```
**Solution:**  
This often happens due to version mismatch or TypeScript incompatibility.
- Upgrade `@vitejs/plugin-vue` and `vite` to latest versions compatible with your TypeScript.
- Delete `node_modules` and `package-lock.json`, then run `npm install`.
- If the error persists, check the offending `.d.ts` line for syntax errors.

---

### Recommendations and Code Fixes

#### 1. Fix your `tsconfig.*.json` files:
- Ensure valid `lib` options.
- Add `"incremental": true` if using `tsBuildInfoFile`, or remove `tsBuildInfoFile`.

#### 2. Upgrade dependencies:
```shell
npm install @vitejs/plugin-vue@latest vite@latest typescript@latest
```
Then, clean and reinstall:
```shell
rm -rf node_modules package-lock.json
npm install
```

#### 3. Re-run the build:
```shell
npm run build
```

---

If you need help with a specific config file, please provide the file path or its contents, and I can give a focused edit.  
Links for reference:
- [tsconfig.node.json (ref: d8a7477e)](https://github.com/hkthao/family-tree-3/blob/d8a7477e93849b2ce59185ecd981db3e56e49f00/tsconfig.node.json)
- [tsconfig.app.json (ref: d8a7477e)](https://github.com/hkthao/family-tree-3/blob/d8a7477e93849b2ce59185ecd981db3e56e49f00/tsconfig.app.json)
- [tsconfig.vitest.json (ref: d8a7477e)](https://github.com/hkthao/family-tree-3/blob/d8a7477e93849b2ce59185ecd981db3e56e49f00/tsconfig.vitest.json)

Apply these fixes and your build should succeed. If any error remains, share the updated error and relevant config for targeted help.