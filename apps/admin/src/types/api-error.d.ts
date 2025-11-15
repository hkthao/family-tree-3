export class ApiError {
  message: string;
  statusCode?: number;
  errors?: { [key: string]: string[] };

  constructor(message: string, statusCode?: number, errors?: { [key: string]: string[] }) {
    this.message = message;
    this.statusCode = statusCode;
    this.errors = errors;
  }

  static fromError(error: any): ApiError {
    if (error.response && error.response.data) {
      // Assuming backend sends error details in error.response.data
      const { message, statusCode, errors } = error.response.data;
      return new ApiError(message || error.message, statusCode || error.response.status, errors);
    } else if (error.message) {
      return new ApiError(error.message);
    }
    return new ApiError('An unknown error occurred.');
  }
}
