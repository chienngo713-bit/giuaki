# Hướng dẫn đẩy dự án TOEIC lên Git (GitHub)

Tài liệu này ghi lại đúng các bước đã thực hiện cho repo `c:\CHIENIT\thigk`
và các bước còn lại để đẩy lên GitHub.

---

## 0. Trạng thái repo hiện tại

Toàn bộ các bước đã hoàn tất — repo đã nằm trên GitHub:

| Việc | Kết quả |
| --- | --- |
| `.gitignore` ở gốc repo | Bỏ qua `bin/`, `obj/`, `buildcheck/`, `.dart_tool/`, `.vs/`, `secrets.json`, log, `*.mdf/*.ldf`, ... |
| `.gitattributes` | Chuẩn hoá line ending: lưu LF trong repo, checkout CRLF trên Windows |
| `README.md` | Mô tả dự án, cách chạy backend/frontend |
| `git init -b main` | Repo đã được khởi tạo, nhánh mặc định `main` |
| `git add -A` + `git commit` | 911 file, ~26.4 MB — **không** có file build nào lọt vào |
| `git remote add origin` | `origin` → `https://github.com/chienngo713-bit/giuaki.git` |
| `git push -u origin main` | Đã đẩy thành công — nhánh `main`, commit `d98b390` |

> Kiểm tra nhanh sau này: `git ls-files | Measure-Object -Line` và
> `git ls-files | Select-String '(bin|obj|buildcheck)/'` (phải ra 0 kết quả).

---

## 1. Cấu hình danh tính (bắt buộc, làm 1 lần)

Repo này **đã** được cấu hình danh tính ở mức repo (không đụng cấu hình global của máy):

```powershell
git config user.name    # chienngo713-bit
git config user.email   # 233412358+chienngo713-bit@users.noreply.github.com
```

Email dạng `ID+username@users.noreply.github.com` là email noreply do GitHub cấp: commit được
ghi danh cho tài khoản mà không lộ email thật.

Đổi sang tên/email cá nhân (nếu muốn):

```powershell
# Dùng cho mọi repo trên máy
git config --global user.name  "Nguyen Van A"
git config --global user.email "email-github-cua-ban@gmail.com"

# Hoặc chỉ cho repo này
git config user.name  "Nguyen Van A"
git config user.email "email-github-cua-ban@gmail.com"

# Kiểm tra
git config --list | Select-String "user\."
```

Commit đã push rồi thì đổi danh tính cần viết lại lịch sử:

```powershell
git commit --amend --reset-author --no-edit
git push --force-with-lease origin main
```

Commit **mới** sau này tự động dùng danh tính vừa cấu hình, không cần amend.

> Dùng **đúng email tài khoản GitHub** nếu muốn commit được tính cho tài khoản đó.

---

## 2. Commit đầu tiên ✅ đã xong (`d98b390`)

```powershell
cd c:\CHIENIT\thigk

git add -A
git commit -m "chore: khoi tao repo TOEIC (ABP backend + Flutter frontend)"

# Xem kết quả
git log --oneline --stat -1
git status
```

Từ đây mọi thay đổi đều theo vòng lặp: `git add` → `git commit` → `git push`.

---

## 3. Tạo repository trên GitHub ✅ đã xong

> Repo đã tạo sẵn: <https://github.com/chienngo713-bit/giuaki>
> (Public, nhánh mặc định `main`, tạo **không** kèm README/gitignore/license nên push không xung đột.)

1. Mở <https://github.com/new>
2. **Repository name**: ví dụ `toeic-reading` (hoặc `thigk`)
3. **Public / Private**: tuỳ bạn (đồ án nên để Private, sau này có thể đổi trong Settings)
4. **RẤT QUAN TRỌNG**: **không** tick "Add a README file", "Add .gitignore", "Choose a license"
   → nếu tick, GitHub tạo sẵn 1 commit và push lần đầu sẽ bị `rejected (fetch first)`.
5. Bấm **Create repository** rồi copy URL hiện ra, dạng:
   - HTTPS: `https://github.com/<user>/<repo>.git`
   - SSH: `git@github.com:<user>/<repo>.git`

---

## 4. Thêm remote và push ✅ đã xong

> Remote hiện tại: `origin` → `https://github.com/chienngo713-bit/giuaki.git`
> Lần push đầu đã chạy: `git push -u origin main` → kết quả `* [new branch] main -> main`.

```powershell
cd c:\CHIENIT\thigk

# Đặt tên remote là origin (thay URL bằng URL của bạn)
git remote add origin https://github.com/<user>/<repo>.git

# Nếu đã lỡ thêm sai thì sửa lại:
# git remote set-url origin https://github.com/<user>/<repo>.git

# Kiểm tra
git remote -v

# Push lần đầu: -u để lần sau chỉ cần gõ `git push`
git push -u origin main
```

### Xác thực khi push qua HTTPS

Máy này đã có **Git Credential Manager** (`credential.helper=manager`), nên khi chạy
`git push` sẽ tự bật cửa sổ đăng nhập GitHub (trình duyệt) — đăng nhập một lần, các lần sau
Git nhớ giúp bạn.

Nếu cửa sổ đăng nhập không hiện, dùng **Personal Access Token (PAT)**:

1. GitHub → **Settings → Developer settings → Personal access tokens → Tokens (classic)**
   → **Generate new token (classic)** → tick scope `repo` → Copy token (`ghp_...`).
2. Khi Git hỏi `Username`: nhập username GitHub.
   Khi hỏi `Password`: **dán token** (không phải mật khẩu GitHub).

### Cách khác: dùng SSH

```powershell
# 1. Tạo khoá (Enter liên tục để dùng mặc định)
ssh-keygen -t ed25519 -C "email-github-cua-ban@gmail.com"

# 2. Copy nội dung public key
Get-Content "$env:USERPROFILE\.ssh\id_ed25519.pub" | Set-Clipboard

# 3. GitHub → Settings → SSH and GPG keys → New SSH key → dán vào → Add

# 4. Kiểm tra rồi đổi remote sang SSH
ssh -T git@github.com          # phải thấy: Hi <user>! You've successfully authenticated...
git remote set-url origin git@github.com:<user>/<repo>.git
git push -u origin main
```

---

## 5. Lỗi thường gặp và cách xử lý

| Lỗi | Nguyên nhân | Cách xử lý |
| --- | --- | --- |
| `Please tell me who you are` | Chưa cấu hình `user.name`/`user.email` | Làm mục 1 |
| `remote origin already exists` | Đã thêm remote rồi | `git remote set-url origin <URL>` |
| `Updates were rejected because the remote contains work...` | Repo GitHub có commit sẵn (do tick README/gitignore) | `git pull --rebase origin main` rồi `git push -u origin main` |
| `src refspec main does not match any` | Chưa commit hoặc đang ở nhánh `master` | `git commit ...`; kiểm tra `git branch` rồi `git branch -M main` |
| `fatal: refusing to merge unrelated histories` | Hai lịch sử khác gốc | `git pull origin main --allow-unrelated-histories --rebase` |
| `RPC failed; HTTP 408 / 413` | Mạng yếu hoặc repo lớn | `git config --global http.postBuffer 524288000` rồi push lại |
| `file is 128.00 MB; this exceeds GitHub's file size limit` | Có file >100 MB | Xem mục 6 |
| Push chậm/hỏng do proxy | Proxy công ty | `git config --global http.proxy http://user:pass@host:port` (bỏ bằng `--unset`) |

Khi bị hỏi xung đột trong lúc `pull --rebase`: sửa file → `git add <file>` → `git rebase --continue`.

---

## 6. Kiểm tra dung lượng trước khi push

```powershell
# Tổng dung lượng vùng staged (đã kiểm tra: ~26.4 MB)
git ls-files | ForEach-Object { (Get-Item $_).Length } | Measure-Object -Sum

# File lớn nhất trong repo
git ls-files | ForEach-Object { [pscustomobject]@{ F = $_; MB = [math]::Round((Get-Item $_).Length/1MB, 2) } } |
  Sort-Object MB -Descending | Select-Object -First 10
```

File Word `backend\assets\Reading v04.docx` (~23 MB) là file nặng nhất — vẫn dưới hạn 100 MB/file
nên push được. Nếu sau này có file >100 MB:

- Dùng **Git LFS**: `git lfs install` → `git lfs track "*.zip"` → `git add .gitattributes` → commit.
- Hoặc bỏ hẳn file khỏi repo nếu không cần thiết.

---

## 7. Những việc cần lưu ý về nội dung đã commit

**Chuỗi kết nối trong `appsettings.json` có mật khẩu `sa/123qwe`:**
`backend\Chien\10.2.0\aspnet-core\src\Chien.Web.Host\appsettings.json`

Đây là giá trị mặc định của template ASP.NET Boilerplate (không phải mật khẩu máy chủ thật),
nên vẫn an toàn khi để trong repo công khai. Nếu sau này bạn đổi bằng mật khẩu thật:

1. Đưa chuỗi kết nối ra file riêng không commit:
   `appsettings.Secrets.json` (đặt `"Copy to Output Directory"` nếu cần) — đã nằm trong `.gitignore` dạng `secrets.json`.
2. Hoặc dùng **user secrets**:
   ```powershell
   cd src\Chien.Web.Host
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Password=..."
   ```

---

## 8. Bỏ file đã lỡ commit

```powershell
# Bỏ khỏi git nhưng giữ file trên máy
git rm -r --cached backend/tools/buildcheck
git commit -m "chore: bo artefact build khoi repo"

# Nếu file đã nằm trong commit cũ và cần xoá hẳn khỏi lịch sử:
git filter-repo --path backend/tools/buildcheck --invert-paths   # cần cài git-filter-repo
# hoặc đơn giản nhất khi repo còn mới: xoá .git và init lại
```

---

## 9. Vòng làm việc hằng ngày

```powershell
git status                       # xem đang thay đổi gì
git add .                        # hoặc: git add <đường-dẫn-cụ-thể>
git commit -m "feat: them API cham diem TOEIC"
git push
```

Làm việc nhóm:

```powershell
git checkout -b feat/toeic-api   # tạo nhánh mới
# ... code ...
git push -u origin feat/toeic-api   # rồi mở Pull Request trên GitHub
```

Nếu đang có sẵn code chưa commit mà muốn đồng bộ với team:

```powershell
git pull --rebase origin main    # lấy code mới, đặt commit của mình lên trên
git push
