# Code Signing Quick Start

## The Problem
Windows shows "Windows protected your PC" because your installer isn't digitally signed.

## Quick Solutions

### Option 1: Do Nothing (Free)
**Users can bypass the warning:**
1. Click "More info"
2. Click "Run anyway"

**Add this to your README/download page:**
```
Windows may show a security warning. Click "More info" then "Run anyway" to install.
This is normal for unsigned software. Your antivirus still protects you.
```

### Option 2: Get a Certificate ($85-500/year)

#### Recommended: Standard Certificate (~$85-200/year)
**Best for:** Personal projects, small businesses, hobbyist developers

**Providers:**
- [Sectigo](https://sectigo.com/ssl-certificates-tls/code-signing) - $85/year
- [SSL.com](https://www.ssl.com/certificates/code-signing/) - $100/year  
- [DigiCert](https://www.digicert.com/signing/code-signing-certificates) - $200/year

**Timeline:**
- Purchase: 5 minutes
- Identity verification: 1-3 days
- Receive certificate: Immediate after verification
- Build SmartScreen reputation: 2-6 weeks

#### Premium: EV Certificate (~$300-500/year)
**Best for:** Commercial software, businesses, paid applications

**Key benefit:** No SmartScreen warnings from day 1!

**Providers:**
- DigiCert EV: $500/year
- SSL.com EV: $350/year

**Timeline:**
- Purchase: 5 minutes
- Strict verification: 3-7 days (business docs required)
- Hardware token shipped: 2-5 days
- No reputation building needed!

---

## How to Sign (Once You Have a Certificate)

### Step 1: Install Windows SDK
Download from: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/

This includes SignTool.exe needed for signing.

### Step 2: Save Your Certificate
1. Download your certificate (.pfx file)
2. Create folder: `certificates\`
3. Save as: `certificates\codesign.pfx`
4. **Important:** Add `certificates/` to .gitignore!

### Step 3: Set Your Password
```batch
set CODE_SIGN_PASSWORD=YourCertificatePassword
```

Or add permanently to Windows environment variables.

### Step 4: Run the Signing Script
```batch
sign-installer.bat
```

### Step 5: Verify
Right-click the installer → Properties → Digital Signatures tab
You should see your signature!

---

## Integrated Build Process

Once set up, you can sign automatically during build:

**Edit `build-installer.bat`** and add before the `:end` label:

```batch
REM Step 5: Sign installer (if certificate available)
if exist "certificates\codesign.pfx" (
    if defined CODE_SIGN_PASSWORD (
        echo.
        echo [5/5] Signing installer...
        call sign-installer.bat
    )
)
```

Then just run:
```batch
build-installer.bat
```

Everything will be built AND signed automatically!

---

## Certificate Comparison

| Type | Cost/Year | Setup Time | SmartScreen | Best For |
|------|-----------|------------|-------------|----------|
| **None** | $0 | 0 min | Warning shows | Testing, personal use |
| **Standard** | $85-200 | 1-3 days | Warning at first* | Small projects |
| **EV** | $300-500 | 5-10 days | No warning | Professional/commercial |

*Standard certificates need 2-6 weeks to build SmartScreen reputation

---

## Recommended Path for Different Use Cases

### Hobby Project / Free Software
1. Start with **no certificate** (Option 1)
2. Add clear instructions for users
3. If project grows, get Standard certificate later

### Small Business / Paid Software
1. Get **Standard certificate** from Sectigo ($85/year)
2. Sign all releases consistently
3. Build SmartScreen reputation over 4-6 weeks
4. Upgrade to EV later if needed

### Professional / Commercial Software
1. Get **EV certificate** from DigiCert or SSL.com
2. No waiting for reputation
3. Instant trust from Windows
4. Professional appearance

---

## Files Included

- **CODE_SIGNING_GUIDE.md** - Complete detailed guide
- **sign-installer.bat** - Ready-to-use signing script
- **SIGNING_QUICKSTART.md** - This file

---

## Next Steps

**If you want to sign your installer:**

1. Choose a certificate provider (I recommend Sectigo for $85/year)
2. Purchase "Code Signing Certificate"
3. Complete identity verification (email + phone call)
4. Download certificate when approved (1-3 days)
5. Run `sign-installer.bat`
6. Distribute signed installer!

**If you prefer to skip signing:**

1. Add installation instructions to your README
2. Tell users to click "More info" → "Run anyway"
3. Many legitimate small apps do this

---

## Cost-Benefit Analysis

**$85/year Standard Certificate:**
- ✅ Professional appearance
- ✅ Shows your name as verified publisher
- ✅ Users trust it more
- ✅ SmartScreen warning goes away (eventually)
- ❌ Initial cost
- ❌ Annual renewal
- ❌ Takes time to build reputation

**$0 No Certificate:**
- ✅ No cost
- ✅ Works immediately  
- ❌ Looks less professional
- ❌ Users may not trust it
- ❌ SmartScreen warning always shows

For a professional application like TimeTracker, I'd recommend getting at least a Standard certificate ($85/year) to build trust with users.

---

## Questions?

See **CODE_SIGNING_GUIDE.md** for detailed information on:
- Certificate providers comparison
- Step-by-step signing instructions
- Troubleshooting common issues
- Security best practices
- Automated signing in your build process
