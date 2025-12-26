import re
import unicodedata

def clean_ocr_text(text: str) -> str:
    if not text:
        return ""

    # 1. Normalize Unicode
    text = unicodedata.normalize("NFC", text)

    # 2. Remove repeated noise (.... ---- **** |||)
    text = re.sub(r"([^\w\s])\1{1,}", " ", text)

    # 3. Allow Vietnamese chars + digits + punctuation
    text = re.sub(
        r"[^0-9a-zA-ZÀ-ỹ\s.,:/()\-%]",
        " ",
        text
    )

    # 4. Line-level filtering (drop garbage lines)
    cleaned_lines = []
    for line in text.splitlines():
        line = re.sub(r"\s+", " ", line).strip()

        if not line:
            continue

        # Alpha ratio heuristic
        alpha_ratio = sum(c.isalpha() for c in line) / max(len(line), 1)
        if alpha_ratio < 0.25:
            continue

        cleaned_lines.append(line)

    return "\n".join(cleaned_lines)
