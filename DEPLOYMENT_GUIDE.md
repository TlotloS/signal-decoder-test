# Signal Decoder Assessment - Deployment Guide

Complete setup instructions for the hidden test assessment infrastructure.

## Architecture Overview

Three repositories work together:

1. **Private Test Repo** (`b1sa-signal-decoder-assessment-tests`) - Hidden tests
2. **Starter Template** (`signal-decoder-starter`) - Given to candidates
3. **Reference Implementation** (current repo) - Complete working solution

## Prerequisites

- GitHub account
- Git installed
- .NET 8 SDK (for testing)

---

## Step 1: Deploy Private Test Repository

### 1.1 Create GitHub Repository

Create new **PRIVATE** repository: `b1sa-signal-decoder-assessment-tests`

### 1.2 Push Test Code

```bash
cd ../b1sa-signal-decoder-assessment-tests
git init
git add .
git commit -m "Initial commit: 24 API integration tests"
git remote add origin https://github.com/YOUR_ORG/b1sa-signal-decoder-assessment-tests.git
git branch -M main
git push -u origin main
```

### 1.3 Verify

- Confirm repository is **PRIVATE**
- Check all test files are present

---

## Step 2: Create Personal Access Token (PAT)

### 2.1 Generate Token

1. GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. "Generate new token (classic)"
3. Configure:
   - Name: `Signal Decoder Assessment Tests`
   - Expiration: 1 year
   - Scope: **repo** (Full control of private repositories)
4. **Copy and save the token securely**

---

## Step 3: Deploy Starter Template

### 3.1 Create GitHub Repository

Create new **PUBLIC** repository: `signal-decoder-starter`

### 3.2 Push Starter Code

```bash
cd ../signal-decoder-starter
git init
git add .
git commit -m "Initial commit: Signal Decoder starter template"
git remote add origin https://github.com/YOUR_ORG/signal-decoder-starter.git
git branch -M main
git push -u origin main
```

### 3.3 Add PAT Secret

1. Go to repo Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `ASSESSMENT_PAT`
4. Value: (paste your PAT)
5. Add secret

### 3.4 Update Workflow File

Edit `.github/workflows/assessment.yml` if needed:

```yaml
repository: YOUR_ORG/b1sa-signal-decoder-assessment-tests
```

---

## Step 4: Test the Workflow

### 4.1 Test Locally

```bash
# Clone reference implementation
cd ..
git clone https://github.com/YOUR_ORG/b1sa-signal-decoder-assessment.git test-verify

cd test-verify

# Copy tests
mkdir -p tests
cp -r ../b1sa-signal-decoder-assessment-tests/AssessmentTests tests/SignalDecoder.AssessmentTests

# Build and test
dotnet build
dotnet test tests/SignalDecoder.AssessmentTests
```

Expected: **24/24 tests pass**

### 4.2 Test GitHub Actions

Option A: Create test fork
```bash
# Fork signal-decoder-starter
# Add ASSESSMENT_PAT secret to fork
# Clone reference implementation to fork
# Push and check Actions tab
```

Option B: Test commit
```bash
cd signal-decoder-starter
echo "test" > test.txt
git add test.txt
git commit -m "Test workflow"
git push
# Check Actions tab
```

---

## Step 5: Candidate Setup

### Option A: Template Repository (Recommended)

1. Go to `signal-decoder-starter` settings
2. Enable "Template repository"
3. Candidates use "Use this template" to create their repo
4. **You must add ASSESSMENT_PAT to each candidate repo**

### Option B: Organization Secrets

1. Create organization
2. Add organization secret `ASSESSMENT_PAT`
3. Candidates fork within organization
4. Automatic access to secret

### Option C: Manual Per Candidate

1. Candidate forks starter repo
2. Give you collaborator access
3. You add `ASSESSMENT_PAT` secret
4. They can now push and test

---

## Step 6: Candidate Instructions

Send candidates:

```
Repository: https://github.com/YOUR_ORG/signal-decoder-starter

Instructions:
1. Fork the repository
2. Implement your solution (see docs/SPECIFICATION.md)
3. Push your code
4. Check Actions tab for results
```

---

## Maintenance

### Update Tests

```bash
cd b1sa-signal-decoder-assessment-tests
# Edit test files
git add .
git commit -m "Update tests"
git push
```

Changes apply to new test runs automatically.

### Rotate PAT

Every 6-12 months:
1. Generate new PAT
2. Update ASSESSMENT_PAT in all repos
3. Revoke old PAT

### Troubleshooting

**"Authentication failed"**
- PAT expired → Generate new PAT

**"Repository not found"**
- Check repository name in workflow
- Verify PAT has repo access

**All tests failing**
- Candidate not implementing API correctly
- Direct them to API_CONTRACT.md

---

## Security Checklist

- [ ] Test repo is PRIVATE
- [ ] PAT has minimal scopes (repo only)
- [ ] PAT stored as secret (never committed)
- [ ] Workflow doesn't leak test details
- [ ] ADMIN_GUIDE.md in private repo only

---

## Repository URLs

```bash
# Private tests (hidden)
https://github.com/YOUR_ORG/b1sa-signal-decoder-assessment-tests

# Starter template (public)
https://github.com/YOUR_ORG/signal-decoder-starter

# Reference implementation (optional, private)
https://github.com/YOUR_ORG/b1sa-signal-decoder-assessment
```

---

## Quick Reference

**Test Count:** 24 tests
- DecoderCorrectnessTests: 6
- DecoderToleranceTests: 3
- DecoderPerformanceTests: 3
- DecoderValidationTests: 4
- GeneratorAndSimulatorTests: 7
- RoundTripTests: 1

**Required Structure:**
```
candidate-repo/
├── src/
│   ├── SignalDecoder.Api/
│   │   └── Program.cs (must have: public partial class Program {})
│   ├── SignalDecoder.Domain/
│   └── SignalDecoder.Application/
└── tests/
    └── SignalDecoder.AssessmentTests/ (copied by Actions)
```

**Performance Requirements:**
- 10 devices: < 1s
- 15 devices: < 3s
- 20 devices: < 5s

---

## Summary

You now have complete assessment infrastructure with:

1. Hidden tests in private repository
2. Starter template for candidates
3. Automated GitHub Actions testing
4. Secure PAT-based access
5. Full documentation

Candidates can fork, implement, push, and receive automated feedback without seeing test code!
