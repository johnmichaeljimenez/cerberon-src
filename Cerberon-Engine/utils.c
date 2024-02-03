#include <raylib.h>
#include <raymath.h>
#include "utils.h"
#include <math.h>

Vector2 Vector2RotateAround(Vector2 position, Vector2 origin, float rotation)
{
	Vector2 t = Vector2Subtract(position, origin);
	Vector2 rotatedVector;
	rotatedVector.x = t.x * cosf(rotation) - t.y * sinf(rotation);
	rotatedVector.y = t.x * sinf(rotation) + t.y * cosf(rotation);

	return Vector2Add(rotatedVector, origin);
}

unsigned long ToHash(unsigned char* str) {
	unsigned long hash = 5381;
	int c;

	while (c = *str++) {
		hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
	}

	return hash;
}

void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale, Vector2 offset, Color color)
{
	Rectangle src = (Rectangle){ 0, 0, t->Texture.width, t->Texture.height };
	Rectangle dest = (Rectangle){ pos.x, pos.y, t->Texture.width * scale, t->Texture.height * scale };
	Vector2 origin = (Vector2){ dest.width / 2, dest.height / 2 };

	origin = Vector2Add(origin, (Vector2) { dest.width* offset.x, dest.height* offset.y });

	DrawTexturePro(t->Texture, src, dest, origin, rotation * RAD2DEG, color);
}

bool HasFlag(int from, int to)
{
	return from & to;
}

Color ColorBrightness01(Color color, float amt)
{
	return ColorBrightness(color, Remap(amt, 0, 1, -1, 0));
}

Vector2 GetNormalVector(Vector2 from, Vector2 to)
{
	Vector2 diff = Vector2Subtract(to, from);
	return Vector2Normalize((Vector2) { -diff.y, diff.x });
}

float WrapAngle(float from, float to)
{
	float max_angle = PI * 2;
	float difference = fmodf(to - from, max_angle);
	return fmodf(2 * difference, max_angle) - difference;
}

float LerpAngle(float r1, float r2, float t)
{
	return r1 + WrapAngle(r1, r2) * t;
}

bool CheckCollisionPointRecRotated(Vector2 point, Rectangle rec, float angle)
{
	Vector2 origin = (Vector2){ rec.x, rec.y };
	rec.x -= (rec.width / 2);
	rec.y -= (rec.height / 2);
	point = Vector2RotateAround(point, origin, -angle);

	return CheckCollisionPointRec(point, rec);
}

void DrawRenderTextureToScreen(Texture* t, float scale)
{
	Rectangle srcRec = { 0, 0, t->width, -t->height };
	Rectangle destRect = (Rectangle){ 0, 0, t->width * scale , t->height * scale };
	Vector2 origin = { 0,0 };
	DrawTexturePro(*t, srcRec, destRect, origin, 0, WHITE);
}

void DrawBlobShadow(Vector2 pos, float radius, float intensity)
{
	Color c1 = ColorAlpha(BLACK, intensity);
	Color c2 = (Color){ 0,0,0,0 };

	DrawCircleGradient(pos.x, pos.y, radius, c1, c2);
}

float ClampRelativeAngle(float angle, float reference, float min_offset, float max_offset)
{
	float relative_angle = angle - reference;
	float clamped_relative_angle = fmaxf(fminf(relative_angle, max_offset), min_offset);

	return reference + clamped_relative_angle;
}

bool GetCircleTangent(Vector2 from, Vector2 circlePos, float radius, Vector2* tanA, Vector2* tanB)
{
	Vector2 dir = Vector2Subtract(from, circlePos);
	float m = Vector2Length(dir);

	if (m < radius)
		return false;

	m = fminf(radius, m);

	float angle = atan2(dir.y, dir.x);

	float tangentAngle = asin(radius / m);

	tanA->x = circlePos.x + radius * cos(angle + tangentAngle);
	tanA->y = circlePos.y + radius * sin(angle + tangentAngle);

	tanB->x = circlePos.x + radius * cos(angle - tangentAngle);
	tanB->y = circlePos.y + radius * sin(angle - tangentAngle);

	return true;
}

Color LerpColor(Color a, Color b, float t)
{
	Vector3 ca = ColorToHSV(a);
	Vector3 cb = ColorToHSV(b);
	Vector3 output;

	output.x = Lerp(ca.x, cb.x, t);
	output.y = Lerp(ca.y, cb.y, t);
	output.z = Lerp(ca.z, cb.z, t);

	Color c = ColorFromHSV(output.x, output.y, output.z);
	c.a = 255;

	return c;
}

bool StartsWith(const char* pre, const char* str)
{
	size_t lenpre = strlen(pre),
		lenstr = strlen(str);
	return lenstr < lenpre ? false : memcmp(pre, str, lenpre) == 0;
}

float GetRandomValueFloat(float min, float max)
{
	return Lerp(min, max, (float)GetRandomValue(0, 100) / 100.0f);
}

//Taken from Raylib examples
void DrawTextRect(const char* text, Rectangle rec, float fontSize, bool wordWrap, Color tint)
{
    Font font = GetFontDefault();
    int length = TextLength(text);  // Total length in bytes of the text, scanned by codepoints in loop

    float textOffsetY = 0;          // Offset between lines (on line break '\n')
    float textOffsetX = 0.0f;       // Offset X to next character to draw

    float scaleFactor = fontSize / (float)font.baseSize;     // Character rectangle scaling factor

    // Word/character wrapping mechanism variables
    enum { MEASURE_STATE = 0, DRAW_STATE = 1 };
    int state = wordWrap ? MEASURE_STATE : DRAW_STATE;

    int startLine = -1;         // Index where to begin drawing (where a line begins)
    int endLine = -1;           // Index where to stop drawing (where a line ends)
    int lastk = -1;             // Holds last value of the character position

    for (int i = 0, k = 0; i < length; i++, k++)
    {
        // Get next codepoint from byte string and glyph index in font
        int codepointByteCount = 0;
        int codepoint = GetCodepoint(&text[i], &codepointByteCount);
        int index = GetGlyphIndex(font, codepoint);

        // NOTE: Normally we exit the decoding sequence as soon as a bad byte is found (and return 0x3f)
        // but we need to draw all of the bad bytes using the '?' symbol moving one byte
        if (codepoint == 0x3f) codepointByteCount = 1;
        i += (codepointByteCount - 1);

        float glyphWidth = 0;
        if (codepoint != '\n')
        {
            glyphWidth = (font.glyphs[index].advanceX == 0) ? font.recs[index].width * scaleFactor : font.glyphs[index].advanceX * scaleFactor;

            if (i + 1 < length) glyphWidth = glyphWidth + 2.0f;
        }

        // NOTE: When wordWrap is ON we first measure how much of the text we can draw before going outside of the rec container
        // We store this info in startLine and endLine, then we change states, draw the text between those two variables
        // and change states again and again recursively until the end of the text (or until we get outside of the container).
        // When wordWrap is OFF we don't need the measure state so we go to the drawing state immediately
        // and begin drawing on the next line before we can get outside the container.
        if (state == MEASURE_STATE)
        {
            // TODO: There are multiple types of spaces in UNICODE, maybe it's a good idea to add support for more
            // Ref: http://jkorpela.fi/chars/spaces.html
            if ((codepoint == ' ') || (codepoint == '\t') || (codepoint == '\n')) endLine = i;

            if ((textOffsetX + glyphWidth) > rec.width)
            {
                endLine = (endLine < 1) ? i : endLine;
                if (i == endLine) endLine -= codepointByteCount;
                if ((startLine + codepointByteCount) == endLine) endLine = (i - codepointByteCount);

                state = !state;
            }
            else if ((i + 1) == length)
            {
                endLine = i;
                state = !state;
            }
            else if (codepoint == '\n') state = !state;

            if (state == DRAW_STATE)
            {
                textOffsetX = 0;
                i = startLine;
                glyphWidth = 0;

                // Save character position when we switch states
                int tmp = lastk;
                lastk = k - 1;
                k = tmp;
            }
        }
        else
        {
            if (codepoint == '\n')
            {
                if (!wordWrap)
                {
                    textOffsetY += (font.baseSize + font.baseSize / 2) * scaleFactor;
                    textOffsetX = 0;
                }
            }
            else
            {
                if (!wordWrap && ((textOffsetX + glyphWidth) > rec.width))
                {
                    textOffsetY += (font.baseSize + font.baseSize / 2) * scaleFactor;
                    textOffsetX = 0;
                }

                // When text overflows rectangle height limit, just stop drawing
                if ((textOffsetY + font.baseSize * scaleFactor) > rec.height) break;

                // Draw current character glyph
                if ((codepoint != ' ') && (codepoint != '\t'))
                {
                    DrawTextCodepoint(font, codepoint, (Vector2) { rec.x + textOffsetX, rec.y + textOffsetY }, fontSize, tint);
                }
            }

            if (wordWrap && (i == endLine))
            {
                textOffsetY += (font.baseSize + font.baseSize / 2) * scaleFactor;
                textOffsetX = 0;
                startLine = endLine;
                endLine = -1;
                glyphWidth = 0;
                k = lastk;

                state = !state;
            }
        }

        if ((textOffsetX != 0) || (codepoint != ' ')) textOffsetX += glyphWidth;  // avoid leading spaces
    }
}