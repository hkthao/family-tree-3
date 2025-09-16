 npm run lint --prefix frontend

> frontend@0.0.0 lint
> eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --ignore-path ../.gitignore

=============

WARNING: You are currently running a version of TypeScript which is not officially supported by @typescript-eslint/typescript-estree.

You may find that it works just fine, or you may not.

SUPPORTED TYPESCRIPT VERSIONS: >=4.7.4 <5.6.0

YOUR TYPESCRIPT VERSION: 5.8.3

Please only submit bug reports when using the officially supported version.

=============

/Users/kimthaohuynh/Documents/Projects/family-tree-3/frontend/vite.config.ts
   1:46  error  Missing semicolon  semi
   3:36  error  Missing semicolon  semi
   4:37  error  Missing semicolon  semi
   5:51  error  Missing semicolon  semi
   6:42  error  Missing semicolon  semi
  20:3   error  Missing semicolon  semi

/Users/kimthaohuynh/Documents/Projects/family-tree-3/frontend/vitest.config.ts
   1:41  error  Missing semicolon  semi
   2:74  error  Missing semicolon  semi
   3:39  error  Missing semicolon  semi
  31:2   error  Missing semicolon  semi

✖ 10 problems (10 errors, 0 warnings)
  10 errors and 0 warnings potentially fixable with the `--fix` option.