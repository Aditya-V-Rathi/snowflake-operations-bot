# Stage 1: Build the Angular app
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json .
RUN npm install

# Copy everything else and build
COPY . .
RUN npm run build --prod

# Stage 2: Serve with Nginx
FROM nginx:alpine AS runtime

# Copy built Angular files to Nginx's serve directory
COPY --from=build /app/dist/my-app/browser /usr/share/nginx/html

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]