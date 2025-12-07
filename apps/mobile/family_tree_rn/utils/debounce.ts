export function debounce<T extends (...args: any[]) => any>(func: T, delay: number): T {
  let timeout: ReturnType<typeof setTimeout> | null;

  return function(this: any, ...args: Parameters<T>) {
    const context = this;
    if (timeout) {
      clearTimeout(timeout);
    }
    timeout = setTimeout(() => {
      timeout = null;
      func.apply(context, args);
    }, delay);
  } as T;
}
