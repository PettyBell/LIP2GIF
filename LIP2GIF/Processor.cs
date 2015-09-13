using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Data.SQLite;
using System.IO;
using ImageMagick;


namespace RMP {

	class Processor : ApplicationContext {
		const String APP_NAME = "LIP2GIF";

		public Processor() {
			this.Start();
		}

		/**
		 * 監視・変換処理を開始する
		 */
		public async void Start() {
			NotifyIcon icon = ShowNotificationIcon();
			String path = ChooseTargetFilePath();
			if (path == null) {
				icon.Visible = false;
				System.Threading.Thread.CurrentThread.Abort();
				return;
			}

			// クリスタが起動していない場合は起動を待つ
			// （クリスタの終了をトリガにしてGIFを作成するため）
			System.Diagnostics.Process.Start(path);
			while (!IsClipStudioPaintRunning()) System.Threading.Thread.Sleep(1000);

			String tmpDir = path + "$" + APP_NAME + @"\";
			Directory.CreateDirectory(tmpDir);

			// 画像の更新日時が変わっていたらプレビュー画像を書き出す（クリスタが終了するまで繰り返し）
			String lastImageSize = "";
			while (IsClipStudioPaintRunning()) {
				String imageSize = "";
				try {
					imageSize = new FileInfo(path).LastWriteTime.ToString();
				} catch (Exception e) { }
				if (imageSize != "" && imageSize != lastImageSize) {
					byte[] pngBytes = new byte[0];
					try {
						pngBytes = GetPNGDataFromLIP(path);
					} catch (Exception e) {
					}
					if (pngBytes.Length != 0) {
						File.WriteAllBytes(tmpDir + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", pngBytes);
						icon.Icon = GenerateCounterIcon(Directory.GetFiles(tmpDir).Length);
						lastImageSize = imageSize;
					}
				}
				System.Threading.Thread.Sleep(1000);
			}

			// GIFアニメーションを生成
			icon.Visible = false;
			ProcessDialog dialog = new ProcessDialog();
			String dstPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), APP_NAME + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".gif");
			dialog.Show();
			await Task.Run(() => {
				GenerateAnimationGIF(tmpDir, dstPath);
			});
			dialog.Hide();

			// キャッシュを削除
			foreach (String file in Directory.GetFiles(tmpDir)) {
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}
			Directory.Delete(tmpDir);

			if (File.Exists(dstPath)) {
				System.Diagnostics.Process.Start("explorer.exe", "/select, " + dstPath);
			}
			Application.Exit();
		}

		/**
		 * 指定されたディレクトリ内にある画像からGIFアニメーションを生成する
		 * @param {String} コマ画像が入っているディレクトリのパス
		 * @return {Boolean} 成功した場合はtrue
		 */
		Boolean GenerateAnimationGIF(String tmpDir, String dstPath) {
			String[] files = Directory.GetFiles(tmpDir);
			if (files.Length == 0) return false;
			using (MagickImageCollection collection = new MagickImageCollection()) {
				MagickImage canvas = new MagickImage(files[files.Length - 1]);
				canvas.AnimationDelay = 250;
				canvas.Scale((int)(canvas.Width * 0.5), (int)(canvas.Height * 0.5));
				collection.Add(canvas);

				int perFrame = (int)Math.Ceiling(600.0 / files.Length);
				foreach (String file in files) {
					canvas = new MagickImage(file);
					canvas.AnimationDelay = perFrame;
					canvas.Scale((int)(canvas.Width * 0.5), (int)(canvas.Height * 0.5));
					collection.Add(canvas);
				}

				collection.Optimize();
				collection.Write(dstPath);
			};
			return true;
		}

		/**
		 * .lipファイルからプレビュー画像を取得する
		 * @param {String} .lipファイルのパス
		 * @return {byte[]} PNGデータ
		 */
		byte[] GetPNGDataFromLIP(String filePath) {
			String url = "Data Source=" + filePath + ";Version=3";
			using (var con = new SQLiteConnection(url)) {
				con.Open();
				SQLiteCommand cmd = con.CreateCommand();
				cmd.CommandText = "SELECT ImageData FROM CanvasPreview";
				byte[] data = (byte[])cmd.ExecuteScalar();
				cmd.Dispose();
				con.Close();
				con.Dispose();
				return data;
			}
		}

		/**
		 * ファイルを開くダイアログを開く
		 * @return {String} 選択されたファイルのパス
		 */
		String ChooseTargetFilePath() {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "CLIP STUDIO PAINT(*.lip)|*.lip";
			dialog.Title = "作業アニメを作成する CLIP STUDIO PAINT ファイルを開いてください";
			if (dialog.ShowDialog() == DialogResult.OK) {
				return dialog.FileName;
			} else {
				return null;
			}
		}

		/**
		 * クリスタが実行されているかどうか確認する
		 * @return {Boolean} 実行されている
		 */
		Boolean IsClipStudioPaintRunning() {
			Process[] processes = Process.GetProcesses();
			Boolean isRunning = false;
			foreach (Process process in processes) {
				if (process.ProcessName == @"CLIPStudioPaint") {
					isRunning = true;
				}
				process.Dispose();
			}
			return isRunning;
		}

		/**
		 * 通知領域にアイコンを登録する
		 */
		NotifyIcon ShowNotificationIcon() {
			var icon = new System.Windows.Forms.NotifyIcon();
			icon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LIP2GIF.icon.ico"));
			icon.Visible = true;
			icon.Text = APP_NAME;
			return icon;
		}

		/**
		 * 通知領域の数字アイコンを生成する
		 * @param {int} 番号
		 * @return {Icon} アイコン
		 */
		Icon GenerateCounterIcon(int count) {
			String content = count >= 1000 ? "999" : count.ToString();
			int fontSize = content.Length == 3 ? 22 : 30;
			Bitmap canvas = new Bitmap(64, 64);
			Graphics g = Graphics.FromImage(canvas);
			Font font = new Font("Arial Black", fontSize);
			g.DrawString(content, font, Brushes.White, 0, 0);
			font.Dispose();
			g.Dispose();
			return Icon.FromHandle(canvas.GetHicon());
		}

	}
}
