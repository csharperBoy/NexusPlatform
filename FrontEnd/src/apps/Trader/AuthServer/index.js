const express = require("express");
const cors = require("cors");
const axios = require("axios").default;
const { wrapper } = require("axios-cookiejar-support");
const { CookieJar } = require("tough-cookie");
const cheerio = require("cheerio");
const crypto = require("crypto");

const app = express();
app.use(cors());
app.use(express.json());

const AUTH_BASE = "https://login.emofid.com";
const CLIENT_ID = "easy_pkce";
const REDIRECT_URI = "https://d.easytrader.ir/auth-callback";
const SCOPE = "easy2_api mts_api openid profile login_delegation-api";
const UA =
  "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36";

function base64url(buf) {
  return buf
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

function generatePKCE() {
  const verifier = base64url(crypto.randomBytes(32));
  const challenge = base64url(
    crypto.createHash("sha256").update(verifier).digest(),
  );
  return { verifier, challenge };
}

function generateState() {
  return crypto.randomBytes(16).toString("hex");
}

app.post("/api/login", async (req, res) => {
  const { username, password } = req.body || {};
  if (!username || !password) {
    return res.status(400).json({ error: "username and password required" });
  }

  const jar = new CookieJar();
  const client = wrapper(
    axios.create({
      jar,
      withCredentials: true,
      maxRedirects: 0,
      validateStatus: (s) => s < 400,
      headers: { "user-agent": UA },
    }),
  );

  const log = (msg) => console.log(`[login] ${msg}`);

  try {
    const { verifier, challenge } = generatePKCE();
    const state = generateState();

    /* ─── ۱. GET /connect/authorize ─── */
    const authorizeUrl =
      `${AUTH_BASE}/connect/authorize?` +
      new URLSearchParams({
        client_id: CLIENT_ID,
        redirect_uri: REDIRECT_URI,
        response_type: "code",
        scope: SCOPE,
        state,
        code_challenge: challenge,
        code_challenge_method: "S256",
      }).toString();

    log("step 1: authorize");
    let r = await client.get(authorizeUrl);
    if (r.status !== 303 && r.status !== 302) {
      throw new Error(`unexpected authorize status: ${r.status}`);
    }

    /* ─── ۲. GET /Login → antiforgery token ─── */
    const loginPath = r.headers.location;
    const loginUrl = loginPath.startsWith("http")
      ? loginPath
      : AUTH_BASE + loginPath;

    log("step 2: get login form");
    r = await client.get(loginUrl);
    if (r.status !== 200) throw new Error(`login page status: ${r.status}`);

    const $ = cheerio.load(r.data);
    const token = $("input[name='__RequestVerificationToken']").val();
    if (!token) throw new Error("__RequestVerificationToken not found");

    /* ─── ۳. POST /Login ─── */
    log("step 3: submit credentials");
    const formBody = new URLSearchParams({
      Username: username,
      Password: password,
      __RequestVerificationToken: token,
    }).toString();

    r = await client.post(loginUrl, formBody, {
      headers: { "content-type": "application/x-www-form-urlencoded" },
    });

    if (r.status !== 302 && r.status !== 303) {
      throw new Error(
        `login failed with status ${r.status} — احتمالاً رمز اشتباه یا کپچا`,
      );
    }

    /* ─── ۴. GET /connect/authorize/callback ─── */
    let redirectUrl = r.headers.location;
    if (redirectUrl.startsWith("/")) redirectUrl = AUTH_BASE + redirectUrl;

    log("step 4: follow to auth callback");
    r = await client.get(redirectUrl);
    if (r.status !== 302 && r.status !== 303) {
      throw new Error(`callback status: ${r.status}`);
    }

    /* ─── ۵. URL نهایی با code ─── */
    const finalUrl = r.headers.location;
    const finalParams = new URL(finalUrl).searchParams;
    const code = finalParams.get("code");
    const returnedState = finalParams.get("state");

    if (returnedState !== state) throw new Error("state mismatch");
    if (!code) throw new Error("no code in callback");

    log(`got code: ${code.slice(0, 20)}...`);

    /* ─── ۶. POST /connect/token ─── */
    log("step 6: exchange code for token");
    const tokenBody = new URLSearchParams({
      grant_type: "authorization_code",
      redirect_uri: REDIRECT_URI,
      code,
      code_verifier: verifier,
      client_id: CLIENT_ID,
    }).toString();

    r = await client.post(`${AUTH_BASE}/connect/token`, tokenBody, {
      headers: { "content-type": "application/x-www-form-urlencoded" },
    });

    if (r.status !== 200) throw new Error(`token exchange failed: ${r.status}`);
    if (!r.data.access_token) throw new Error("no access_token in response");

    log(`✅ success — token expires in ${r.data.expires_in}s`);

    res.json({
      access_token: r.data.access_token,
      id_token: r.data.id_token,
      expires_in: r.data.expires_in,
      scope: r.data.scope,
    });
  } catch (e) {
    console.error("[login] ❌", e.message);
    res.status(500).json({ error: e.message });
  }
});

app.get("/api/health", (_req, res) => res.json({ ok: true }));

const PORT = 5000;
app.listen(PORT, () => {
  console.log(`🔐 Login server running on http://localhost:${PORT}`);
});