FROM node:22-alpine AS build

COPY . .

RUN corepack enable

RUN yarn

RUN yarn build 

FROM nginx:1.31-alpine

COPY --from=build /dist /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
