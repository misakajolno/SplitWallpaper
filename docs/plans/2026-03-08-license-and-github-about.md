# SplitWallpaper License and GitHub About Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an MIT license to the repository and document the license in both README files while preparing a GitHub-ready repository description.

**Architecture:** Keep the change focused on repository metadata and public-facing documentation. Add a standard root `LICENSE` file, then update the Chinese and English READMEs with short license sections that point back to the root license file.

**Tech Stack:** Markdown, plain text, GitHub repository metadata conventions

---

### Task 1: Add the root license file

**Files:**
- Create: `LICENSE`

**Step 1: Add the standard MIT license text**

Create `LICENSE` with the standard MIT license template for the current copyright holder.

**Step 2: Verify the file exists**

Run: `rg -n "^MIT License$" LICENSE`
Expected: one match at the top of the file

### Task 2: Document the license in the Chinese README

**Files:**
- Modify: `README.md`

**Step 1: Add a short license section**

Append a `## 许可证` section near the end of the document.

**Step 2: Link the section to the root license**

Mention that the repository is released under `MIT`, and point readers to `LICENSE`.

**Step 3: Verify the heading exists**

Run: `rg -n "^## 许可证$" README.md`
Expected: one match

### Task 3: Document the license in the English README

**Files:**
- Modify: `README.en.md`

**Step 1: Add a short license section**

Append a `## License` section near the end of the document.

**Step 2: Link the section to the root license**

Mention that the repository is released under `MIT`, and point readers to `LICENSE`.

**Step 3: Verify the heading exists**

Run: `rg -n "^## License$" README.en.md`
Expected: one match

### Task 4: Prepare the GitHub About metadata

**Files:**
- Reference: `docs/plans/2026-03-08-license-and-github-about-design.md`

**Step 1: Use the recommended one-line description**

Prepare:

`Windows app for splitting the primary desktop into left and right wallpapers.`

**Step 2: Prepare topic suggestions**

Prepare:

`windows`, `wpf`, `dotnet`, `wallpaper`, `desktop-app`

**Step 3: Verify the final response includes both**

Check the final handoff before sending it.
