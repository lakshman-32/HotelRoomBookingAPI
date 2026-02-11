$files = Get-ChildItem -Path "Views" -Filter "*.cshtml" -Recurse
foreach ($file in $files) {
    echo "Processing $($file.Name)..."
    $content = Get-Content -Path $file.FullName -Raw
    $newContent = $content
    
    # helper: replace A with B only if A is found
    if ($newContent -match "HotelRoomBookingAPI.Models.ViewModels") {
        $newContent = $newContent -replace "HotelRoomBookingAPI.Models.ViewModels", "HotelRoomBookingAPI.Models.Web.ViewModels"
    }
    if ($newContent -match "HotelRoomBookingAPI.Models.DTOs") {
        $newContent = $newContent -replace "HotelRoomBookingAPI.Models.DTOs", "HotelRoomBookingAPI.Models.Web.DTOs"
    }
    # Also handle root models if they show up as HotelRoomBookingAPI.Models.SomeClass
    # But be careful not to match HotelRoomBookingAPI.Models.Web
    # Regex lookahead/behind is useful but simple string replace might handle specific cases
    
    # Replace "HotelRoomBookingAPI.Models.Room" with "HotelRoomBookingAPI.Models.Web.Room"
    # List of known web models: Room, Booking, User, Floor, BuildingsMaster, RoomType, ClientKind, Review...
    $webModels = @("Room", "Booking", "User", "Floor", "BuildingsMaster", "RoomType", "ClientKind", "RoomStatus")
    
    foreach ($model in $webModels) {
        $pattern = "HotelRoomBookingAPI.Models.$model"
        $replacement = "HotelRoomBookingAPI.Models.Web.$model"
        if ($newContent -match $pattern) {
             # verify it's not already .Web.$model (though simple match check above helps, regex is safer)
             # actually -replace uses regex
             # escape .
             $regexPattern = "HotelRoomBookingAPI\.Models\.$model\b"
             $newContent = $newContent -replace $regexPattern, $replacement
        }
    }

    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed namespaces in $($file.Name)"
    }
}
