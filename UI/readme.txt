UI is far far away for now (not needed and IMGUI is enough during development)
however, I plan to take advantage of letterboxing so that I don't have headaches in UI scaling or aspect ratios.
basically, all UI rects are normalized 0-1 position and size, and during development it will just be in a JSON or raw txt file and every time it's changed, it just updates the ingame rect coordinates according to element id
but the UI itself it rendered outside the letterbox render texture to keep the crisp display even if it relies on the letterboxed coordinates
that's it, and i dont even need dynamic layout, viewports or scrollbars anyway. every UI element is just rect+ID and their value like sprite or text to render + click event