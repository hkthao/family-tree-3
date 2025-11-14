# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Production stage (IMPORTANT: use nginx:alpine, not stable-alpine)
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Install envsubst
RUN apk add --no-cache gettext

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy entrypoint
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Copy config template BEFORE build assets
COPY public/config.template.js ./config.template.js

# Copy FE build output
COPY --from=build /app/dist .

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
