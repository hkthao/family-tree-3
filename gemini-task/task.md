src/plugins/axios.ts:63:25 - error TS2339: Property 'isSuccess' does not exist on type 'Result<T, ApiError>'.
  Property 'isSuccess' does not exist on type '{ ok: false; error: ApiError; }'.

63       if (backendResult.isSuccess) {
                           ~~~~~~~~~

src/plugins/axios.ts:64:33 - error TS2339: Property 'value' does not exist on type 'Result<T, ApiError>'.
  Property 'value' does not exist on type '{ ok: false; error: ApiError; }'.

64         return ok(backendResult.value as T);
                                   ~~~~~

src/plugins/axios.ts:66:34 - error TS2339: Property 'error' does not exist on type 'Result<T, ApiError>'.
  Property 'error' does not exist on type '{ ok: true; value: T; }'.

66         return err(backendResult.error as ApiError);
                                    ~~~~~

src/services/chat/api.chat.service.ts:4:35 - error TS2307: Cannot find module './chat/chat.service.interface' or its corresponding type declarations.

4 import type { IChatService } from './chat/chat.service.interface';
                                    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

src/services/chat/mock.chat.service.ts:9:11 - error TS2741: Property 'context' is missing in type '{ response: string; sessionId: string; model: string; createdAt: string; }' but required in type 'ChatResponse'.

9     const mockResponse: ChatResponse = {
            ~~~~~~~~~~~~

  src/types/chat.d.ts:3:3
    3   context: string[];
        ~~~~~~~
    'context' is declared here.

src/services/naturalLanguageInput.service.ts:2:24 - error TS2307: Cannot find module '@/types/result' or its corresponding type declarations.

2 import { Result } from '@/types/result';
                         ~~~~~~~~~~~~~~~~

src/services/naturalLanguageInput.service.ts:21:33 - error TS2339: Property 'data' does not exist on type 'Result<string, ApiError>'.
  Property 'data' does not exist on type '{ ok: false; error: ApiError; }'.

21       const jsonData = response.data;
                                   ~~~~

src/services/userProfile/mock.userProfile.service.ts:28:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

28       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.

src/services/userProfile/mock.userProfile.service.ts:38:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

38       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.

src/services/userProfile/mock.userProfile.service.ts:49:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

49       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.

src/plugins/axios.ts:63:25 - error TS2339: Property 'isSuccess' does not exist on type 'Result<T, ApiError>'.
  Property 'isSuccess' does not exist on type '{ ok: true; value: T; }'.

63       if (backendResult.isSuccess) {
                           ~~~~~~~~~

src/plugins/axios.ts:64:33 - error TS2339: Property 'value' does not exist on type 'Result<T, ApiError>'.
  Property 'value' does not exist on type '{ ok: false; error: ApiError; }'.

64         return ok(backendResult.value as T);
                                   ~~~~~

src/plugins/axios.ts:66:34 - error TS2339: Property 'error' does not exist on type 'Result<T, ApiError>'.
  Property 'error' does not exist on type '{ ok: true; value: T; }'.

66         return err(backendResult.error as ApiError);
                                    ~~~~~

src/services/chat/api.chat.service.ts:4:35 - error TS2307: Cannot find module './chat/chat.service.interface' or its corresponding type declarations.

4 import type { IChatService } from './chat/chat.service.interface';
                                    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

src/services/chat/mock.chat.service.ts:9:11 - error TS2741: Property 'context' is missing in type '{ response: string; sessionId: string; model: string; createdAt: string; }' but required in type 'ChatResponse'.

9     const mockResponse: ChatResponse = {
            ~~~~~~~~~~~~

  src/types/chat.d.ts:3:3
    3   context: string[];
        ~~~~~~~
    'context' is declared here.

src/services/userProfile/mock.userProfile.service.ts:28:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

28       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.

src/services/userProfile/mock.userProfile.service.ts:38:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

38       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.

src/services/userProfile/mock.userProfile.service.ts:49:27 - error TS2741: Property 'name' is missing in type 'ApiError' but required in type 'Error'.

49       return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
                             ~~~~~

  node_modules/typescript/lib/lib.es5.d.ts:1076:5
    1076     name: string;
             ~~~~
    'name' is declared here.