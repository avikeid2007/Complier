# Turbo C 3.0 IDE — Development Checklist

Track implementation progress for all pending features and fixes.

**Last Updated:** April 8, 2026  
**Project Status:** Beta (All menu items ✅, Advanced features pending ⏳)

---

## 🔴 HIGH PRIORITY (Critical / Blocks Usage)

### Core Infrastructure
- [x] Resource leaks fixed (timers disposed on unload)
- [x] ActiveEditor safe fallback (no crash on empty)
- [x] Box-drawing borders responsive (Grid-based stretching)
- [x] Block cursor dynamic sizing (scales with font)
- [x] Multi-tab editor (TabView implementation)
- [x] Add + button for new tabs
- [x] Vertical/horizontal stretching (full area usage)

**Status:** ✅ **COMPLETE**

---

## 🟠 MEDIUM PRIORITY (Feature Completeness)

### Debug Menu (4 items)
| Item | Handler | Dialog | Status |
|------|---------|--------|--------|
| Evaluate/Modify (Ctrl+F4) | ✅ | ✅ | ✅ |
| Watches | ✅ | ✅ | ✅ |
| Toggle Breakpoint (Ctrl+F8) | ✅ | ✅ | ✅ |
| Breakpoints | ✅ | ✅ | ✅ |

**Status:** ✅ Stubs complete (GDB integration needed for full functionality)

---

### Project Menu (6 items)
| Item | Handler | Dialog | Status |
|------|---------|--------|--------|
| Open Project | ✅ | ✅ | ✅ |
| Close Project | ✅ | ✅ | ✅ |
| Add Item | ✅ | ✅ | ✅ |
| Delete Item | ✅ | ✅ | ✅ |
| Local Options | ✅ | ✅ | ✅ |
| Include Files | ✅ | ✅ | ✅ |

**Status:** ✅ Stubs complete (full project system needs .PRJ format design)

---

### Options Menu (3 items incomplete)
| Item | Handler | Dialog | Status |
|------|---------|--------|--------|
| Compiler | ✅ | ✅ | ✅ |
| Directories | ✅ | ✅ | ✅ |
| Linker | ✅ | ✅ | ✅ |
| Make | ✅ | ✅ | ✅ |
| Arguments | ✅ | ✅ | ✅ |
| Environment | ✅ | ✅ | ✅ |
| Save Options | ✅ | ✅ | ✅ |
| Retrieve Options | ✅ | ✅ | ✅ |

**Status:** ✅ **COMPLETE**

---

### Run Menu (2 items missing)
| Item | Handler | Status |
|------|---------|--------|
| Run Program | ✅ | ✅ |
| User Screen (Alt+F5) | ✅ | ✅ |
| Program Reset (Ctrl+F2) | ✅ | ✅ |
| Go to Cursor (F4) | ✅ | ✅ (stub — needs debugger) |
| Trace Into (F7) | ✅ | ✅ (stub — needs debugger) |
| Step Over (F8) | ✅ | ✅ (stub — needs debugger) |

**Status:** ✅ Stubs complete (trace/step require GDB)

---

### Syntax Highlighting (8+ functions missing)

#### String Functions (6)
- [x] `strlen()`
- [x] `strcmp()`
- [x] `strcpy()`
- [x] `strcat()`
- [x] `strchr()`
- [x] `strstr()`

#### Math Functions (7)
- [x] `sin()`
- [x] `cos()`
- [x] `tan()`
- [x] `sqrt()`
- [x] `pow()`
- [x] `ceil()`
- [x] `floor()`

#### Character/Type Functions (6)
- [x] `isalpha()`
- [x] `isdigit()`
- [x] `isspace()`
- [x] `toupper()`
- [x] `tolower()`
- [x] `atoi()` / `atof()`

#### I/O Functions (6)
- [x] `fopen()`
- [x] `fclose()`
- [x] `fprintf()`
- [x] `fscanf()`
- [x] `fgets()`
- [x] `fputs()`

#### Memory Functions (2)
- [x] `calloc()`
- [x] `realloc()`

#### Time Functions (4)
- [x] `time()`
- [x] `clock()`
- [x] `localtime()`
- [x] `strftime()`

**Estimated effort:** 1 hour (add to SyntaxHighlighter.cs)  
**Blocked by:** None  
**Priority:** Medium (polish)

---

### Help System (3 items)
| Item | Handler | Status |
|------|---------|--------|
| Help Contents (F1) | ✅ | ✅ |
| Help Index | ✅ | ✅ |
| Topic Search (Ctrl+F1) | ✅ | ✅ |
| Previous Topic (Alt+F1) | ✅ | ✅ |
| Help on Help | ✅ | ✅ |

**Status:** ✅ **COMPLETE**

---

### Search Features (2 items)
| Item | Handler | Status |
|------|---------|--------|
| Find (Ctrl+F) | ✅ | ✅ |
| Replace (Ctrl+H) | ✅ | ✅ |
| Find Again | ✅ | ✅ |
| Find Previous | ✅ | ✅ |
| Find Procedure | ✅ | ✅ |
| Find Error | ✅ (click in panel) | ✅ |

**Status:** ✅ **COMPLETE**

---

### Compiler Enhancements (1 item)
- [x] Expose 30s timeout in Options > Compiler
  - [x] Add numeric input field in Compiler dialog
  - [x] Save setting to config file
  - [x] Apply to ProcessRunner

**Estimated effort:** 1 hour  
**Blocked by:** None  
**Priority:** Low

---

## 🟡 LOW PRIORITY (Polish / Nice-to-have)

### File Menu Stubs (2 items)
- [x] Save All — Saves all open tabs with unsaved changes
- [x] Print — Informational message (no OS print dialog)

**Status:** ✅ **COMPLETE**

---

### Edit Menu Stubs (2 items)
- [x] Copy Example — Inserts a Hello World stub at cursor
- [x] Show Clipboard — Displays clipboard text in message panel

**Status:** ✅ **COMPLETE**

---

### Window Management Menu (8 items)
| Item | Handler | Status | Effort |
|------|---------|--------|--------|
| Size/Move (Ctrl+F5) | ✅ | ✅ (OS resize hint) | - |
| Zoom (F5) | ✅ | ✅ (OS maximize hint) | - |
| Tile | ✅ | ✅ (tabs hint) | - |
| Cascade | ✅ | ✅ (tabs hint) | - |
| Output Panel | ✅ | ✅ (toggle message panel) | - |
| Watch Panel | ✅ | ✅ (GDB stub) | - |
| Register Panel | ✅ | ✅ (GDB stub) | - |
| Project/Notes | ✅ | ✅ (disabled) | - |

**Status:** ✅ **COMPLETE**

---

### Compile Menu (1 item)
- [x] Primary C File... — Shows configured primary file; links to Options > Make

**Status:** ✅ **COMPLETE**

---

### System Menu Stubs (2 items)
- [x] Clear Desktop — Clears the message panel
- [x] Repaint Desktop — Triggers a layout pass on RootGrid

**Status:** ✅ **COMPLETE**

---

## 📊 Implementation Summary

### By Category
| Category | Total | Done | Pending | % Complete |
|----------|-------|------|---------|-------------|
| **Debug Menu** | 4 | 4 | 0 | 100% |
| **Project Menu** | 6 | 6 | 0 | 100% |
| **Options Menu** | 8 | 8 | 0 | 100% |
| **Run Menu** | 6 | 6 | 0 | 100% |
| **Syntax HL** | 50+ | 50+ | 0 | 100% |
| **Help System** | 5 | 5 | 0 | 100% |
| **Search** | 6 | 6 | 0 | 100% |
| **File Menu** | 2 | 2 | 0 | 100% |
| **Edit Menu** | 2 | 2 | 0 | 100% |
| **Window Mgmt** | 8 | 8 | 0 | 100% |
| **Compile Menu** | 1 | 1 | 0 | 100% |
| **System Menu** | 2 | 2 | 0 | 100% |

**Total: 100 items | 100 Complete (100%) | 0 Pending (0%)**

---

### By Priority & Effort
| Priority | Count | Est. Effort | Status |
|----------|-------|-------------|--------|
| 🔴 High | 8 | — | ✅ DONE |
| 🟠 Medium | 30+ | ~25 hours | ✅ DONE |
| 🟡 Low | 8+ | ~20 hours | ✅ DONE |

---

## 🎯 Recommended Implementation Order

### **Phase 1: Quick Wins** ✅ COMPLETE
- [x] Expand syntax highlighting (20+ C functions)
- [x] Add "Find Again" / "Find Previous" to Search menu
- [x] Expose compiler timeout in Options

**Impact:** High (visual polish, UX improvement)  
**Effort:** Low

---

### **Phase 2: Menu Completeness** ✅ COMPLETE
- [x] Debug menu stubs (4 dialogs)
- [x] Run menu stubs (Program Reset + Trace/Step stubs)
- [x] Linker, Make, Environment, Save/Retrieve Options dialogs
- [x] Find Procedure (Search menu)

**Impact:** Medium (menu coverage)  
**Effort:** Medium

---

### **Phase 3: Feature Expansion** (15+ hours)
- [ ] Project system (`.PRJ` format)
- [ ] Help > Topic Search (extended content)
- [ ] Output/Watch panels (GDB integration)

**Impact:** Medium (advanced features)  
**Effort:** High

---

### **Phase 4: Polish** (20+ hours)
- [ ] Window tiling/cascading (multi-window)
- [ ] Complete Debug menu (actual GDB debugging)
- [ ] Save/Retrieve options persistence (already done — verify)

**Impact:** Low (convenience)  
**Effort:** High

---

## 📝 Notes

### Known Blockers
- **GDB integration** — Trace/Step (F7/F8) require external debugger
- **Project format** — Need `.PRJ` file spec for project system
- **Panel architecture** — Window/Output/Watch panels need multi-window redesign

### Testing Needed
- [ ] Memory profiling (check for leaks in long sessions)
- [ ] Large file handling (>10MB)
- [ ] Tab stress test (50+ tabs open)
- [ ] Compiler timeout behavior
- [ ] Menu keyboard navigation (Alt+key chains)

### Documentation Needed
- [ ] API docs for extending syntax highlighter
- [ ] Help content library (for F1, Ctrl+F1)
- [ ] Project file format specification
- [ ] Configuration file format

---

## 🚀 Getting Started

Pick a task from **Phase 1** to get started:

```bash
# Example: Add syntax highlighting for strlen, strcmp, etc.
# File: Services/SyntaxHighlighter.cs
# Lines: ~50 (in _keywords set)
# Time: 15-30 min
```

---

**Questions?** See [README.md](README.md) or create a GitHub Issue!
