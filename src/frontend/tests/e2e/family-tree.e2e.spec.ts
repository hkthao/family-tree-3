import { test, expect } from '@playwright/test';

test.describe.serial('Family Tree E2E Tests', () => {
  // Biến để lưu trữ ID của cây gia phả và thành viên được tạo trong quá trình test
  let familyId: string;
  let memberId: string;

  // Test case 1: Đăng nhập bằng Auth0 với tài khoản hợp lệ
  test('should allow a user to login via Auth0', async ({ page }) => {
    // Điều hướng đến trang đăng nhập của ứng dụng
    await page.goto('http://localhost/');

    // TODO: Cần thêm data-testid cho các trường nhập liệu và nút đăng nhập của Auth0
    // Nhập email và mật khẩu của tài khoản test Auth0
    // Giả định Auth0 có trường nhập email với data-testid="auth0-email-input"
    // và trường nhập mật khẩu với data-testid="auth0-password-input"
    // và nút đăng nhập với data-testid="auth0-login-button"
    await page.fill('input[type="email"]', 'test@example.com'); // Thay bằng email test Auth0
    await page.fill('input[type="password"]', 'TestPassword123!'); // Thay bằng mật khẩu test Auth0
    await page.click('button[type="submit"]');

    // Chờ cho đến khi điều hướng đến trang dashboard sau khi đăng nhập thành công
    // TODO: Cần thêm data-testid cho trang dashboard hoặc một phần tử duy nhất trên dashboard
    await page.waitForURL('http://localhost/dashboard');
    await expect(page.locator('[data-testid="dashboard-title"]')).toBeVisible();
    console.log('Đăng nhập thành công và chuyển đến trang dashboard.');
  });

  // Test case 2: Tạo một cây gia phả mới
  test('should allow a user to create a new family tree', async ({ page }) => {
    // Điều hướng đến trang quản lý gia phả
    await page.goto('http://localhost/family-management');

    // Nhấn nút "Thêm mới Gia đình"
    await page.click('[data-testid="add-new-family-button"]');

    // Điền thông tin cây gia phả mới
    const familyName = `Test Family ${Date.now()}`;
    const familyDescription = 'Mô tả cho cây gia phả test.';
    await page.fill('[data-testid="family-name-input"]', familyName);
    await page.fill('[data-testid="family-description-input"]', familyDescription);

    // Nhấn nút "Lưu"
    await page.click('[data-testid="save-family-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log(`Đã tạo cây gia phả mới: ${familyName}`);

    // TODO: Cần lấy familyId từ URL hoặc từ API response sau khi tạo thành công
    // Tạm thời, chúng ta sẽ tìm kiếm tên gia phả trong danh sách để xác nhận
    await expect(page.locator(`text=${familyName}`)).toBeVisible();
  });

  // Test case 3: Thêm thành viên (cha/mẹ/con) vào cây gia phả
  test('should allow a user to add a member to the family tree', async ({ page }) => {
    // Điều hướng đến trang chi tiết cây gia phả (cần familyId)
    // TODO: Cần lấy familyId từ test case trước hoặc từ API
    // Tạm thời, chúng ta sẽ điều hướng đến trang quản lý gia phả và chọn gia phả vừa tạo
    await page.goto('http://localhost/family-management');
    const familyName = `Test Family`; // Cần lấy tên gia phả đã tạo
    await page.click(`text=${familyName}`); // Click vào gia phả vừa tạo

    // Nhấn nút "Thêm Thành viên Mới"
    await page.click('[data-testid="add-new-member-button"]');

    // Điền thông tin thành viên mới
    const memberFirstName = 'Thành viên';
    const memberLastName = `Test ${Date.now()}`;
    await page.fill('[data-testid="member-first-name-input"]', memberFirstName);
    await page.fill('[data-testid="member-last-name-input"]', memberLastName);
    await page.selectOption('[data-testid="member-gender-select"]', 'Male'); // Chọn giới tính
    // TODO: Cần thêm các trường khác như ngày sinh, nơi sinh, v.v.

    // Nhấn nút "Lưu"
    await page.click('[data-testid="save-member-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log(`Đã thêm thành viên mới: ${memberFirstName} ${memberLastName}`);

    // TODO: Cần lấy memberId từ URL hoặc từ API response sau khi tạo thành công
    // Tạm thời, chúng ta sẽ tìm kiếm tên thành viên trong danh sách để xác nhận
    await expect(page.locator(`text=${memberFirstName} ${memberLastName}`)).toBeVisible();
  });

  // Test case 4: Cập nhật thông tin thành viên
  test('should allow a user to update a member', async ({ page }) => {
    // Điều hướng đến trang quản lý thành viên
    await page.goto('http://localhost/member-management');

    // TODO: Cần chọn thành viên vừa tạo để cập nhật
    // Tạm thời, chúng ta sẽ click vào nút chỉnh sửa của thành viên đầu tiên trong danh sách
    await page.click('[data-testid="edit-member-button"]');

    // Cập nhật thông tin thành viên
    const updatedPhone = '0123456789';
    await page.fill('[data-testid="member-phone-input"]', updatedPhone);

    // Nhấn nút "Lưu"
    await page.click('[data-testid="save-member-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã cập nhật thông tin thành viên.');

    // TODO: Xác nhận thông tin đã cập nhật hiển thị đúng
  });

  // Test case 5: Xem chi tiết cây gia phả
  test('should allow a user to view family tree details', async ({ page }) => {
    // Điều hướng đến trang xem cây gia phả
    // TODO: Cần familyId để điều hướng chính xác
    await page.goto('http://localhost/family-tree');

    // TODO: Xác nhận cây gia phả hiển thị đúng
    await expect(page.locator('[data-testid="family-tree-canvas"]')).toBeVisible();
    console.log('Đã xem chi tiết cây gia phả.');
  });

  // Test case 6: Xóa cây gia phả
  test('should allow a user to delete a family tree', async ({ page }) => {
    // Điều hướng đến trang quản lý gia phả
    await page.goto('http://localhost/family-management');

    // TODO: Cần chọn cây gia phả vừa tạo để xóa
    // Tạm thời, chúng ta sẽ click vào nút xóa của cây gia phả đầu tiên trong danh sách
    await page.click('[data-testid="delete-family-button"]');

    // Xác nhận xóa
    await page.click('[data-testid="confirm-delete-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã xóa cây gia phả.');

    // TODO: Xác nhận cây gia phả không còn trong danh sách
  });

  // Test case 7: Đăng xuất
  test('should allow a user to logout', async ({ page }) => {
    // Nhấn nút đăng xuất
    // TODO: Cần data-testid cho nút đăng xuất
    await page.click('[data-testid="user-menu-button"]'); // Mở menu người dùng
    await page.click('[data-testid="logout-button"]'); // Nhấn nút đăng xuất

    // Chờ cho đến khi điều hướng về trang đăng nhập
    await page.waitForURL('http://localhost/login');
    await expect(page.locator('[data-testid="login-page-title"]')).toBeVisible();
    console.log('Đăng xuất thành công.');
  });
});
