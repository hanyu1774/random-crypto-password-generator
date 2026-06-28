namespace NET.ConsoleApp.RandomPasswords.Flows;

internal sealed class CreateHeader
{
    public ReadOnlySpan<char> run(ReadOnlySpan<char> title)
    {
        const int minimum_padding = 4;
        const int minimum_border_width = 20;
        int title_length = title.Length;
        int content_width = title_length + (minimum_padding * 2);
        int border_width = Math.Max(minimum_border_width, content_width / 2);
        int total_width = content_width + (border_width * 2);

        int padding_total = total_width - title_length - (border_width * 2);
        int left_padding = padding_total / 2;
        int right_padding = padding_total - left_padding;

        int buffer_length = (total_width * 3) + 11;
        char[] buffer = new char[buffer_length];
        Span<char> destination = buffer;
        int position = 0;

        ReadOnlySpan<char> green = "\x1b[32m";
        green.CopyTo(destination[position..]);
        position += green.Length;

        destination.Slice(position, total_width).Fill('#');
        position += total_width;
        destination[position++] = '\n';

        destination.Slice(position, border_width).Fill('#');
        position += border_width;

        destination.Slice(position, left_padding).Fill(' ');
        position += left_padding;

        title.CopyTo(destination[position..]);
        position += title.Length;

        destination.Slice(position, right_padding).Fill(' ');
        position += right_padding;

        destination.Slice(position, border_width).Fill('#');
        position += border_width;
        destination[position++] = '\n';

        destination.Slice(position, total_width).Fill('#');
        position += total_width;

        ReadOnlySpan<char> reset = "\x1b[0m";
        reset.CopyTo(destination[position..]);

        return buffer;
    }
}
