# Sử dụng image Node.js LTS chính thức
FROM node:lts-alpine

# Thiết lập thư mục làm việc trong container
WORKDIR /app

# Copy package.json và package-lock.json (nếu có)
# để tận dụng caching của Docker layers
COPY package*.json ./

# Cài đặt các dependencies của dự án
RUN npm install

# Copy toàn bộ mã nguồn của ứng dụng vào thư mục làm việc
COPY . .

# Mở cổng mà ứng dụng sẽ chạy trên đó
EXPOSE 3000

# Lệnh để khởi chạy ứng dụng
CMD ["node", "index.js"]
