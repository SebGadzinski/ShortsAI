const {createProxyMiddleware, fixRequestBody} = require('http-proxy-middleware');
const {env} = require('process');
var bodyParser = require('body-parser');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${
    env.ASPNETCORE_HTTPS_PORT
}` : env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:51927';

const apiLinks = [
    "/auth/GetSession",
    "/auth/Login",
    "/auth/SignUp",
    "/auth/SendConfirmationEmail",
    "/auth/ConfirmEmail",
    "/auth/SendResetPasswordEmail",
    "/auth/ResetPassword",
    "/auth/Logout",
    "/home/CreateVideo",
    "/home/ServerRunning",
    "/profile",
    "/profile/GetCredits",
    "/profile/PurchaseCredits",
    "/video",
    "/video/GetVideoInfo",
    "/video/Download"
];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(apiLinks, {
        target: target,
        secure: false,
        onProxyReq: fixRequestBody
    });

    app.use(bodyParser.urlencoded({extended: false}));
    app.use(bodyParser.json()); // support json encoded bodies
    app.use(appProxy);
};
