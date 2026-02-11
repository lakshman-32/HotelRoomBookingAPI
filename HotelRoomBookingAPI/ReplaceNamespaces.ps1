$files = Get-ChildItem -Path "Views" -Filter "*.cshtml" -Recurse
foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    if ($content -match "HotelRoomBookingFrontend") {
        $newContent = $content -replace "HotelRoomBookingFrontend", "HotelRoomBookingAPI"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated $($file.Name)"
    }
}
