# Contributing to SmartApp Framework

First off, thank you for considering contributing to SmartApp! It's people like you who make SmartApp a powerful tool for public safety, healthcare, and defense sectors.

As this is a **Mission-Critical Framework**, we maintain high standards for code quality, security, and stability.

---

## 🛡️ Our Code of Conduct
By participating in this project, you agree to maintain a professional and respectful environment. We focus on technical excellence and collaborative problem-solving.

## 🚀 How Can I Contribute?

### Reporting Bugs
* **Check the Issue Tracker:** Ensure the bug hasn't already been reported.
* **Be Specific:** Describe the environment (OS, K8s version, .NET SDK).
* **Provide Logs:** Include any relevant logs from the K8s Orchestrator or the application pods.

### Suggesting Enhancements
* Open an **Issue** with the tag `enhancement`.
* Explain why this feature is needed for critical infrastructure environments.

### Pull Requests (PRs)
1. **Fork the repo** and create your branch from `main`.
2. **Coding Style:** Follow standard C#/.NET naming conventions and clean code principles.
3. **Security First:** Ensure no secrets or hardcoded credentials are included.
4. **Tests:** All PRs must include unit tests or integration tests for new features.
5. **Documentation:** Update the `README.md` or internal docs if you change how the framework works.

---

## 🏗️ Technical Workflow

### 1. Development Environment
To start developing, ensure you have:
* .NET 8.0 SDK
* Docker Desktop (with Kubernetes enabled)
* Access to a local or remote PostgreSQL instance

### 2. Running the Orchestrator
Before submitting a PR, verify that the automated deployment still works:
```bash
dotnet run --project K8sOrchestratorApp
