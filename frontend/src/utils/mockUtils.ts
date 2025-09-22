export function simulateLatency<T>(data: T, error?: string): Promise<T> {
  return new Promise((resolve, reject) => setTimeout(() => {
    if (error) {
      reject(new Error(error));
    } else {
      resolve(data);
    }
  }, 0));
}
