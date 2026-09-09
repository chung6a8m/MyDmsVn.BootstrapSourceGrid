# Known MVP limitations

These are intentional scope boundaries for the first release, not defects in otherwise promised behavior.

- **Native scrollbars:** SourceGrid's native scrollbar implementation remains authoritative. The integration does not replace scrollbar painting or interaction.
- **SourceGrid-native editors:** The existing SourceGrid editor architecture, activation, commit, cancel, validation, and focus behavior remain authoritative. The integration safely aligns active editor font and colors where `UseCellViewProperties` permits it.
- **Native editor chrome:** Some editor borders, popups, and native subcontrols can retain Windows/OS rendering.
- **Custom Views opt out:** Only exact SourceGrid default View singleton identities are automatically substituted. Any explicit consumer View instance is preserved through theme changes.
- **Concrete grid only:** The public control derives from concrete `SourceGrid.Grid`. MVP does not add `BootstrapSourceGridVirtual` or another virtual-grid integration type.
- **Windows-only:** Both supported targets use native Windows Forms: `net48` and `net8.0-windows`.
- **No Bootstrap wrappers:** Rows, columns, cells, ranges, selection, controllers, editors, spans, and scrolling continue to use SourceGrid APIs directly.
- **Cross-grid Bootstrap editor reuse:** Editors created through `BootstrapEditors` are owned by
  the creating grid and must not be assigned to another grid. At the pinned SourceGrid baseline,
  no protected integration callback runs before SourceGrid attaches an editor control and mutates
  its linked-control state, so this unsupported use is not guaranteed to fail before attachment.
  Strict runtime enforcement requires a separately approved SourceGrid pre-attach seam.

See [COMPATIBILITY.md](COMPATIBILITY.md) for the full supported behavior contract.
