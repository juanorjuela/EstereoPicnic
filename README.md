# Un Mundo Distinto - Unity Project

Unity 6 project for "Un Mundo Distinto" game development.

## Getting Started

### Prerequisites

- Unity Editor 6000.2.9f1 or compatible version
- Git installed on your system
- GitHub account (for team members)

### Initial Setup (First Time)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd "Un Mundo Distinto"
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" and select this project folder
   - Unity will import all assets and regenerate the `Library/` folder (this is normal and expected)

3. **Verify Unity Version**
   - Ensure you're using Unity 6000.2.9f1 or compatible version
   - The project uses Universal Render Pipeline (URP)

## Git Workflow

### Simple Collaboration Workflow

We use a simple workflow where everyone works on the `main` branch:

1. **Before starting work:**
   ```bash
   git pull origin main
   ```

2. **Make your changes** in Unity

3. **Commit your changes:**
   ```bash
   git add .
   git commit -m "Description of your changes"
   git push origin main
   ```

### Important Git Rules

- ✅ **DO commit** `.meta` files - they're essential for Unity asset references
- ✅ **DO commit** `ProjectSettings/` folder
- ✅ **DO commit** `Assets/` folder and all its contents
- ✅ **DO commit** `Packages/manifest.json` and `Packages/packages-lock.json`
- ❌ **DON'T commit** `Library/` folder (auto-generated)
- ❌ **DON'T commit** `Temp/` folder (temporary files)
- ❌ **DON'T commit** `Logs/` folder (log files)
- ❌ **DON'T commit** `UserSettings/` folder (personal preferences)

## Unity Collaboration Best Practices

### Scene Editing

⚠️ **Important**: Unity scene files (`.unity`) are binary files that can cause merge conflicts.

**Best Practices:**
- **Coordinate scene edits**: Communicate with your team when you need to edit a scene
- **Use Prefabs**: Create prefabs for reusable objects to minimize scene conflicts
- **Scene merging**: If conflicts occur, Unity has built-in scene merging tools, but coordination is preferred
- **One person per scene**: When possible, have only one person edit a scene at a time

### Asset Workflow

- **Prefabs**: Use prefabs for objects that will be reused or instantiated
- **Scripts**: All C# scripts should be in `Assets/` folder
- **Meta files**: Never delete `.meta` files - they maintain Unity's asset references
- **Asset organization**: Keep assets organized in logical folders

### Communication

- **Before editing scenes**: Check with team if anyone else is working on the same scene
- **After major changes**: Inform team about significant changes
- **Commit messages**: Write clear commit messages describing what you changed

## Project Structure

```
Un Mundo Distinto/
├── Assets/              # All game assets (scenes, scripts, models, etc.)
│   ├── Scenes/         # Unity scene files
│   ├── Settings/       # URP and render pipeline settings
│   └── ...
├── ProjectSettings/    # Unity project configuration
├── Packages/          # Unity package dependencies
└── .gitignore        # Git ignore rules
```

## Setting Up GitHub Repository

### For Repository Owner (First Time Setup)

1. **Create a new repository on GitHub**
   - Go to GitHub.com
   - Click "New repository"
   - Name it (e.g., "un-mundo-distinto")
   - **Don't** initialize with README, .gitignore, or license (we already have these)
   - Click "Create repository"

2. **Connect local repository to GitHub**
   ```bash
   git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
   git branch -M main
   git push -u origin main
   ```

3. **Share repository URL with your team**
   - Give team members access to the repository
   - Share the repository URL for cloning

### For Team Members

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
   cd YOUR_REPO_NAME
   ```

2. **Open in Unity**
   - Unity will automatically regenerate `Library/` folder
   - Wait for Unity to finish importing assets

3. **Start collaborating!**
   - Pull latest changes before starting work: `git pull`
   - Make changes, commit, and push: `git add . && git commit -m "message" && git push`

## Troubleshooting

### Unity shows "Library is missing" error
- This is normal after cloning
- Unity will regenerate the Library folder automatically
- Just wait for Unity to finish importing

### Merge conflicts in scene files
- Try Unity's built-in scene merging tool
- If that fails, coordinate with team to resolve manually
- Consider using prefabs to reduce scene conflicts

### Meta files causing issues
- Never delete `.meta` files manually
- If Unity shows missing script warnings, let Unity regenerate meta files
- Commit all `.meta` files with their corresponding assets

## Resources

- [Unity Version Control](https://docs.unity3d.com/Manual/UnityVersionControl.html)
- [Git Basics](https://git-scm.com/book/en/v2/Getting-Started-Git-Basics)
- [GitHub Collaboration Guide](https://docs.github.com/en/get-started/quickstart/contributing-to-projects)

## Team Members

Add your name here when you join the project!

---

**Happy coding! 🎮**

