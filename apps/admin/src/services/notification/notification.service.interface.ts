export interface INotificationService {
  /**
   * Đồng bộ hóa thông tin người dùng với dịch vụ thông báo Novu.
   *
   * @returns Promise<void>
   */
  syncSubscriber(): Promise<void>;
}
