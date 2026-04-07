#!/usr/bin/env python3
"""Generate dark-themed GitHub-style HTML viewer pages for source files.

Usage:
    python3 generate-file-viewers.py [files-dir]

For every non-.html file in <files-dir> (default: ./files alongside this
script) an HTML companion <filename>.html is written that renders the file
content inside a styled dark code block.
"""
import os
import sys
import html as html_lib


# Map file extensions to Prism.js language classes and display labels
LANG_LABELS = {
    ".Dockerfile": ("docker", "Dockerfile"),
    ".dockerfile": ("docker", "Dockerfile"),
    ".yml": ("yaml", "YAML"),
    ".yaml": ("yaml", "YAML"),
    ".sh": ("bash", "Shell"),
    ".json": ("json", "JSON"),
    ".xml": ("markup", "XML"),
    ".properties": ("ini", "Properties"),
}



def _lang_info(filename: str):
    _, ext = os.path.splitext(filename)
    return LANG_LABELS.get(ext, ("none", "Text"))



def _render(filename: str, content: str) -> str:
        safe_name = html_lib.escape(filename)
        safe_body = html_lib.escape(content)
        lang_class, label = _lang_info(filename)
        # Prism.js CDN links (dark theme)
        prism_css = "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism-tomorrow.min.css"
        prism_js = "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/prism.min.js"
        # Add language plugins for yaml, docker, bash, ini
        prism_plugins = [
                "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-yaml.min.js",
                "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-docker.min.js",
                "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-bash.min.js",
                "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-ini.min.js",
        ]
        plugin_scripts = "\n    ".join(f'<script src="{url}"></script>' for url in prism_plugins)
        return f"""<!DOCTYPE html>
<html lang=\"en\">
<head>
    <meta charset=\"UTF-8\">
    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
    <title>{safe_name}</title>
    <link rel=\"stylesheet\" href=\"{prism_css}\">
    <style>
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        body {{ background: #0d1117; color: #e6edf3;
                     font-family: 'Segoe UI', system-ui, sans-serif; min-height: 100vh; }}
        .header {{ background: #161b22; border-bottom: 1px solid #30363d;
                             padding: 12px 20px; display: flex; align-items: center; gap: 12px; }}
        .filename {{ font-size: 14px; font-weight: 600; color: #e6edf3; }}
        .badge {{ font-size: 11px; background: #21262d; border: 1px solid #30363d;
                            color: #8b949e; padding: 2px 8px; border-radius: 12px; }}
        .back {{ font-size: 13px; color: #58a6ff; text-decoration: none;
                         margin-left: auto; }}
        .back:hover {{ text-decoration: underline; }}
        .code-wrap {{ overflow-x: auto; }}
        pre {{ margin: 0; padding: 20px;
                     font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
                     font-size: 13px; line-height: 1.6; color: #e6edf3;
                     white-space: pre; tab-size: 4; border-radius: 0 0 8px 8px; }}
        code {{ background: none; color: inherit; }}
    </style>
</head>
<body>
    <div class=\"header\">
        <span class=\"filename\">{safe_name}</span>
        <span class=\"badge\">{label}</span>
        <a class=\"back\" href=\"javascript:history.back()\">&#8592; back</a>
    </div>
    <div class=\"code-wrap\"><pre><code class=\"language-{lang_class}\">{safe_body}</code></pre></div>
    <script src=\"{prism_js}\"></script>
    {plugin_scripts}
</body>
</html>"""


def main() -> None:
    files_dir = (
        sys.argv[1]
        if len(sys.argv) > 1
        else os.path.join(os.path.dirname(os.path.abspath(__file__)), "files")
    )
    if not os.path.isdir(files_dir):
        print(f"error: directory not found: {files_dir}", file=sys.stderr)
        sys.exit(1)

    count = 0
    for fname in sorted(os.listdir(files_dir)):
        if fname.endswith(".html"):
            continue
        fpath = os.path.join(files_dir, fname)
        if not os.path.isfile(fpath):
            continue
        with open(fpath, "r", encoding="utf-8", errors="replace") as fh:
            content = fh.read()
        html_out = _render(fname, content)
        out_path = fpath + ".html"
        with open(out_path, "w", encoding="utf-8") as fh:
            fh.write(html_out)
        count += 1

    print(f"Generated {count} HTML viewer(s) in {files_dir}")


if __name__ == "__main__":
    main()
