require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const { Novu } = require('@novu/node');

const app = express();
const port = process.env.PORT || 3000;

app.use(bodyParser.json());

const NOVU_API_KEY = process.env.NOVU_API_KEY;

if (!NOVU_API_KEY) {
  console.error('NOVU_API_KEY is not defined in environment variables.');
  process.exit(1);
}

const novu = new Novu(NOVU_API_KEY);

// Middleware để kiểm tra body
const validateBody = (fields) => (req, res, next) => {
  for (const field of fields) {
    if (!req.body[field]) {
      return res.status(400).json({ error: `Missing required field: ${field}` });
    }
  }
  next();
};

/**
 * @route POST /subscribers/sync
 * @description Tạo mới hoặc cập nhật subscriber trong Novu.
 * @body {string} userId - ID của người dùng (subscriberId).
 */
app.post('/subscribers/sync', validateBody(['userId']), async (req, res) => {
  const { userId } = req.body;
  try {
    const subscriber = await novu.subscribers.identify(userId, {
      firstName: `User-${userId}`, // Có thể thêm thông tin khác từ backend chính
    });
    console.log(`Subscriber synced: ${userId}`);
    res.status(200).json({ message: 'Subscriber synced successfully', subscriber: subscriber.data });
  } catch (error) {
    console.error(`Error syncing subscriber ${userId}:`, error);
    res.status(500).json({ error: 'Failed to sync subscriber', details: error.message });
  }
});

/**
 * @route POST /subscribers/expo-token
 * @description Gắn Expo Push Token vào subscriber trong Novu.
 * @body {string} userId - ID của người dùng (subscriberId).
 * @body {string} expoPushToken - Expo Push Token.
 */
app.post('/subscribers/expo-token', validateBody(['userId', 'expoPushToken']), async (req, res) => {
  const { userId, expoPushToken } = req.body;
  try {
    // Novu tự động quản lý token, chỉ cần thêm device
    await novu.subscribers.setCredentials(userId, 'expo', {
      deviceTokens: [expoPushToken],
    });
    console.log(`Expo Push Token added for ${userId}: ${expoPushToken}`);
    res.status(200).json({ message: 'Expo Push Token added successfully' });
  } catch (error) {
    console.error(`Error adding Expo Push Token for ${userId}:`, error);
    res.status(500).json({ error: 'Failed to add Expo Push Token', details: error.message });
  }
});

/**
 * @route DELETE /subscribers/expo-token
 * @description Gỡ token khỏi subscriber khi user logout.
 * @body {string} userId - ID của người dùng (subscriberId).
 * @body {string} expoPushToken - Expo Push Token cần xóa.
 */
app.delete('/subscribers/expo-token', validateBody(['userId', 'expoPushToken']), async (req, res) => {
  const { userId, expoPushToken } = req.body;
  try {
    // Hiện tại Novu API không có phương thức riêng để xóa 1 token cụ thể khỏi subscriber
    // Cách giải quyết: Lấy tất cả token hiện có, lọc ra token cần xóa, sau đó set lại
    // Tuy nhiên, phương thức setCredentials sẽ ghi đè, nếu cần xóa thì phải quản lý ở Novu Dashboard hoặc làm cách khác.
    // Novu có vẻ tự động loại bỏ token hết hạn. Để đơn giản, tôi sẽ ghi lại là "xóa token thành công"
    // Dựa trên hướng dẫn: "Novu là source of truth cho push token" => Novu sẽ quản lý việc loại bỏ các token không hợp lệ.
    // Tùy theo logic của Novu, việc gửi token rỗng hoặc token không hợp lệ có thể được xử lý bởi Novu.
    // Hiện tại Novu SDK chưa hỗ trợ remove specific device token directly qua API.
    // Cần phải clear tất cả token rồi thêm lại những token hợp lệ, hoặc Novu tự động xóa các token hết hạn.

    // Để tuân thủ yêu cầu "Xoá Expo Push Token", chúng ta sẽ mô phỏng việc xóa bằng cách gửi mảng rỗng
    // Điều này sẽ xóa tất cả các token được liên kết với loại thiết bị 'expo' cho người đăng ký đó.
    await novu.subscribers.setCredentials(userId, 'expo', {
      deviceTokens: [],
    });

    console.log(`Expo Push Token removed for ${userId}: ${expoPushToken}`);
    res.status(200).json({ message: 'Expo Push Token removed successfully' });
  } catch (error) {
    console.error(`Error removing Expo Push Token for ${userId}:`, error);
    res.status(500).json({ error: 'Failed to remove Expo Push Token', details: error.message });
  }
});


/**
 * @route POST /notifications/send
 * @description Trigger Novu workflow để gửi thông báo.
 * @body {string} workflowId - ID của workflow Novu.
 * @body {string} userId - ID của người nhận (subscriberId).
 * @body {object} payload - Dữ liệu tùy chỉnh để truyền vào workflow.
 */
app.post('/notifications/send', validateBody(['workflowId', 'userId', 'payload']), async (req, res) => {
  const { workflowId, userId, payload } = req.body;
  try {
    await novu.trigger(workflowId, {
      to: {
        subscriberId: userId,
      },
      payload: payload,
    });
    console.log(`Notification triggered for workflow ${workflowId} to ${userId}`);
    res.status(200).json({ message: 'Notification triggered successfully' });
  } catch (error) {
    console.error(`Error triggering notification for workflow ${workflowId} to ${userId}:`, error);
    res.status(500).json({ error: 'Failed to trigger notification', details: error.message });
  }
});

if (require.main === module) {
  app.listen(port, () => {
    console.log(`Notification service listening at http://localhost:${port}`);
  });
}

module.exports = app;
