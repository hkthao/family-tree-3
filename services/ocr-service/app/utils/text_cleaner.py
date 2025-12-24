def clean_ocr_text(text: str) -> str:
    """
    Cleans the OCR'd text by removing excessive blank lines and stripping
    whitespace from each line.
    """
    lines = text.splitlines()
    cleaned_lines = []
    for line in lines:
        stripped_line = line.strip()
        if stripped_line:  # Only add non-empty lines
            cleaned_lines.append(stripped_line)
    return "\n".join(cleaned_lines)
