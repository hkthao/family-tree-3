#!/bin/sh

VARS='${VITE_API_BASE_URL} ${VITE_APP_BASE_URL} ${VITE_AUTH0_DOMAIN} ${VITE_AUTH0_CLIENT_ID} ${VITE_AUTH0_AUDIENCE} ${VITE_NOVU_APPLICATION_IDENTIFIER} ${VITE_APP_BUILD_DATE} ${VITE_APP_ENVIRONMENT} ${VITE_APP_COMMIT_ID} ${VITE_API_PUBLIC_KEY}'

envsubst "$VARS" < /usr/share/nginx/html/config.template.js > /usr/share/nginx/html/config.js

exec "$@"
