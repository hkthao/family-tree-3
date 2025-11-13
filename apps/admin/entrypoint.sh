#!/bin/sh
# infra/entrypoint.sh

# Use envsubst to replace variables in the template file
# The list of variables to substitute is passed as an argument to envsubst
envsubst '${VITE_API_BASE_URL},${VITE_AUTH0_DOMAIN},${VITE_AUTH0_CLIENT_ID},${VITE_AUTH0_AUDIENCE},${VITE_NOVU_APPLICATION_IDENTIFIER},${VITE_APP_BUILD_DATE},${VITE_APP_ENVIRONMENT},${VITE_APP_COMMIT_ID}' < /usr/share/nginx/html/config.template.js > /usr/share/nginx/html/config.js

# Execute the original command (nginx)
exec "$@"