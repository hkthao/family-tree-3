# Use an official Node.js runtime as a parent image
FROM node:18-alpine

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY package*.json ./

# Install any needed packages
RUN npm install

# Copy the rest of the application code to the working directory
COPY . .

# Create a directory for local uploads
RUN mkdir -p uploads/local

# Expose the port the app runs on
EXPOSE 3000

# Define the command to run the app
CMD ["node", "src/main.js"]
