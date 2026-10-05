FROM node:22-alpine AS build
WORKDIR /app

RUN apk update && apk upgrade --no-cache

COPY package.json yarn.lock .yarnrc.yml ./

RUN corepack enable

RUN yarn

COPY . .

RUN yarn build 

FROM nginx:1.31-alpine

COPY --from=build /app/dist /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
