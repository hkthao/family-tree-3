export type Result<T, E = Error> =
  | { ok: true; value: T }
  | { ok: false; error: E };

export const ok = <T>(value: T): Result<T, any> => ({ ok: true, value });
export const err = <E>(error: E): Result<any, E> => ({ ok: false, error });
