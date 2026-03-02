# Code Signing Guide for TimeTracker

## Why You're Getting the SmartScreen Warning

Windows SmartScreen shows "Windows protected your PC" for unsigned executables because:
- Your installer has no digital signature
- Windows can't verify the publisher
- It's a security feature to protect users from malware

## Solutions (From Easiest to Most Professional)

### Option 1: Users Can Bypass (Current Situation)
**Cost:** Free
**Time:** 0 minutes

Users can click "More info" → "Run anyway" to bypass the warning.

**Pros:** No cost, works immediately
**Cons:** Looks unprofessional, users may not trust it

---

### Option 2: Self-Signed Certificate (Testing Only)
**Cost:** Free
**Time:** 30 minutes
**Recommended for:** Development/testing only

**⚠️ WARNING:** Self-signed certificates still show SmartScreen warnings to other users. Only useful for your own testing.

**Steps:**
1. Create a self-signed certificate (see below)
2. Install it in your Trusted Root store
3. Sign your installer
4. On YOUR computer only, the warning disappears

**Not recommended** because it doesn't help end users.

---

### Option 3: Standard Code Signing Certificate
**Cost:** $80-$200 per year
**Time:** 1-3 days (verification)
**Recommended for:** Small business, professional releases

**Providers:**
- Sectigo (formerly Comodo): ~$85/year
- DigiCert: ~$200/year
- SSL.com: ~$100/year
- Certum: ~$80/year (EU-based)

**Process:**
1. Purchase certificate from provider
2. Verify your identity (email, phone, sometimes business documents)
3. Receive certificate file (.pfx or .p12)
4. Use SignTool to sign your installer
5. SmartScreen warning goes away after ~100+ downloads build reputation

**Pros:** Removes warning for most users, shows your name as publisher
**Cons:** Annual cost, requires identity verification, SmartScreen reputation takes time

---

### Option 4: EV Code Signing Certificate
**Cost:** $300-$500 per year
**Time:** 3-7 days (strict verification)
**Recommended for:** Professional/commercial software

**Providers:**
- DigiCert: ~$500/year
- SSL.com: ~$350/year
- Sectigo: ~$300/year

**Process:**
1. Purchase EV certificate
2. Strict identity verification (business documents, DUNS number, etc.)
3. Receive USB hardware token (certificate stored on physical device)
4. Sign installer using hardware token
5. **Instant SmartScreen reputation** - no warning from day 1!

**Pros:** No SmartScreen warning immediately, highest trust level, shows your company name
**Cons:** Expensive, requires hardware token, strict verification

---

## Recommended Approach

### For Personal/Free Software:
**Option 1** - Just let users click "More info" → "Run anyway"
- Add instructions to your download page
- Many legitimate small apps do this

### For Paid/Professional Software:
**Option 3** - Standard Code Signing Certificate
- Professional appearance
- Affordable for most developers
- Takes a few weeks to build SmartScreen reputation

### For Commercial/Enterprise Software:
**Option 4** - EV Code Signing Certificate
- No SmartScreen warnings immediately
- Highest level of trust
- Required for kernel drivers

---

## How to Implement Code Signing

### A. Get a Certificate

**For Standard Certificate (Option 3):**
1. Go to Sectigo, DigiCert, or SSL.com
2. Purchase "Code Signing Certificate"
3. Complete identity verification (email, phone call)
4. Download your certificate (.pfx file)
5. Note your certificate password

**For EV Certificate (Option 4):**
1. Go to DigiCert or SSL.com
2. Purchase "EV Code Signing Certificate"
3. Complete extended validation (business docs, phone, DUNS)
4. Receive USB hardware token
5. Install drivers for the token

### B. Install SignTool

SignTool comes with Windows SDK:

```powershell
# Check if you have SignTool
where signtool

# If not found, download Windows SDK:
# https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
```

Common locations:
- `C:\Program Files (x86)\Windows Kits\10\bin\10.0.xxxxx.0\x64\signtool.exe`
- `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\signtool.exe`

### C. Sign Your Installer

**For Standard Certificate (.pfx file):**
```batch
signtool sign /f "path\to\certificate.pfx" /p "YourPassword" /t http://timestamp.sectigo.com /fd sha256 "TimeTrackerSetup_v1.0.41.exe"
```

**For EV Certificate (USB token):**
```batch
signtool sign /n "Your Company Name" /t http://timestamp.sectigo.com /fd sha256 "TimeTrackerSetup_v1.0.41.exe"
```

**Timestamp servers (pick one):**
- Sectigo: `http://timestamp.sectigo.com`
- DigiCert: `http://timestamp.digicert.com`
- GlobalSign: `http://timestamp.globalsign.com`

### D. Verify Signature

```batch
signtool verify /pa "TimeTrackerSetup_v1.0.41.exe"
```

---

## Automated Signing in Your Build Process

Once you have a certificate, update `build-installer.bat`:

```batch
REM Step 5: Sign the installer (new step)
echo.
echo [5/5] Signing installer...
set CERT_PATH=C:\path\to\your\certificate.pfx
set CERT_PASSWORD=YourPasswordHere
set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.xxxxx.0\x64\signtool.exe"

if exist %SIGNTOOL% (
    %SIGNTOOL% sign /f %CERT_PATH% /p %CERT_PASSWORD% /t http://timestamp.sectigo.com /fd sha256 "TimeTrackerSetup_v%VERSION%.exe"
    if errorlevel 1 (
        echo WARNING: Code signing failed!
    ) else (
        echo ✓ Installer signed successfully
    )
) else (
    echo WARNING: SignTool not found - installer not signed
)
```

---

## Certificate Providers Comparison

| Provider | Standard | EV | Reputation | Support |
|----------|----------|-----|------------|---------|
| **Sectigo** | $85/yr | $300/yr | Good | Email |
| **DigiCert** | $200/yr | $500/yr | Excellent | Phone |
| **SSL.com** | $100/yr | $350/yr | Good | Email |
| **Certum** | $80/yr | N/A | Good | Email |

---

## Building SmartScreen Reputation (Standard Cert)

Even with a standard certificate, SmartScreen may show warnings initially. To build reputation:

1. **Sign all versions** - Every release must be signed
2. **Keep certificate valid** - Don't let it expire
3. **Get downloads** - ~100+ downloads helps
4. **Wait** - Takes 2-6 weeks to build reputation
5. **Don't change publishers** - Stick with same certificate

---

## Alternative: Distribute via Microsoft Store

**Cost:** $19 one-time fee
**Pros:** No code signing needed, automatic updates, built-in trust
**Cons:** Microsoft approval process, Store policies, 15% commission on paid apps

For free apps, this can be a good alternative to code signing.

---

## Security Best Practices

**If you get a certificate:**

1. **Protect your .pfx file**
   - Keep it in a secure location
   - Don't commit to git
   - Use strong password
   - Back it up securely

2. **Protect your password**
   - Use environment variables
   - Don't hardcode in scripts
   - Consider using Windows Credential Manager

3. **Timestamp your signatures**
   - Always use `/t` flag with timestamp server
   - Signatures remain valid even after cert expires
   - If timestamp server is down, try another

4. **Verify after signing**
   - Always run `signtool verify` after signing
   - Check signature properties in Windows Explorer
   - Test on a clean Windows machine

---

## For Your Current Situation

**Immediate solution (free):**
Add this to your download page/README:

```markdown
## Installation Note

When you run the installer, Windows may show "Windows protected your PC". 
This is normal for non-signed applications. To install:

1. Click "More info"
2. Click "Run anyway"
3. Follow the installation prompts

This warning appears because the installer is not code-signed. 
Your antivirus will still scan the file for malware.
```

**Long-term solution:**
Purchase a code signing certificate (~$85-200/year) from Sectigo or SSL.com.

---

## Quick Decision Guide

**Choose Option 1 (No signing) if:**
- You're distributing to friends/family
- It's a personal project
- You want to avoid costs
- Users are technical enough to bypass the warning

**Choose Option 3 (Standard cert) if:**
- You're releasing publicly
- You want to look professional
- You can afford $100/year
- You're okay with initial SmartScreen warnings

**Choose Option 4 (EV cert) if:**
- You're selling software
- You need immediate trust
- You have a registered business
- Budget allows $300-500/year

---

## Next Steps

1. **Decide which option fits your needs**
2. **If going with certificate, choose a provider**
3. **Purchase and verify identity**
4. **Receive certificate (3-7 days)**
5. **Install SignTool if not present**
6. **Test signing process**
7. **Update build-installer.bat**
8. **Sign and test installer**

Would you like me to help you set up any of these options?
