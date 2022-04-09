// See https://aka.ms/new-console-template for more information
using CreateAllSongPlaylist;

Console.WriteLine("Beat Saberのインストールパスを入力してください。");

static bool ExitsBSFolder(string? path)
{
    if (string.IsNullOrEmpty(path)) {
        return false;
    }
    return Directory.Exists(path);
}
var path = Console.ReadLine();
while (!ExitsBSFolder(path)) {
    Console.WriteLine("指定したフォルダが見つかりませんでした。もう一度入力してください。");
    path = Console.ReadLine();
}

var task = PlaylistUtil.SaveAllSongs(path);
Console.WriteLine("プレイリスト作成中です。しばらくお待ちください。");
await task;
Console.WriteLine("作成が終了しました。任意のキーを押して終了します。");
Console.ReadLine();