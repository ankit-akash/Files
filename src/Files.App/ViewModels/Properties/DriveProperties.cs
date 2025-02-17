using Files.App.DataModels.NavigationControlItems;
using Files.App.Filesystem;
using Files.App.Filesystem.StorageItems;
using Files.App.Helpers;
using Files.App.Extensions;
using System;
using Windows.Storage.FileProperties;

namespace Files.App.ViewModels.Properties
{
	internal class DriveProperties : BaseProperties
	{
		public DriveItem Drive { get; }

		public DriveProperties(SelectedItemsPropertiesViewModel viewModel, DriveItem driveItem, IShellPage instance)
		{
			ViewModel = viewModel;
			Drive = driveItem;
			AppInstance = instance;
			GetBaseProperties();
		}

		public override void GetBaseProperties()
		{
			if (Drive is null)
				return;

			ViewModel.CustomIconSource = null; //Drive.IconSource;
			ViewModel.IconData = Drive.IconData;
			ViewModel.LoadCustomIcon = false; //Drive.IconSource is not null && Drive.IconData is null;
			ViewModel.LoadFileIcon = Drive.IconData is not null;
			ViewModel.ItemName = Drive.Text;
			ViewModel.OriginalItemName = Drive.Text;
			// Note: if DriveType enum changes, the corresponding resource keys should change too
			ViewModel.ItemType = string.Format("DriveType{0}", Drive.Type).GetLocalizedResource();
		}

		public async override void GetSpecialProperties()
		{
			ViewModel.ItemAttributesVisibility = false;
			var item = await FilesystemTasks.Wrap(() => DrivesManager.GetRootFromPathAsync(Drive.Path));
			BaseStorageFolder diskRoot = await FilesystemTasks.Wrap(() => StorageFileExtensions.DangerousGetFolderFromPathAsync(Drive.Path, item));

			if (ViewModel.LoadFileIcon)
			{
				if (diskRoot is not null)
				{
					ViewModel.IconData = await FileThumbnailHelper.LoadIconFromStorageItemAsync(diskRoot, 80, ThumbnailMode.SingleItem);
				}
				else
				{
					ViewModel.IconData = await FileThumbnailHelper.LoadIconWithoutOverlayAsync(Drive.Path, 80);
				}
				ViewModel.IconData ??= await FileThumbnailHelper.LoadIconWithoutOverlayAsync(Drive.DeviceID, 80); // For network shortcuts
			}

			if (diskRoot is null || diskRoot.Properties is null)
			{
				ViewModel.LastSeparatorVisibility = false;
				return;
			}

			try
			{
				string freeSpace = "System.FreeSpace";
				string capacity = "System.Capacity";
				string fileSystem = "System.Volume.FileSystem";

				var properties = await diskRoot.Properties.RetrievePropertiesAsync(new[] { freeSpace, capacity, fileSystem });

				ViewModel.DriveCapacityValue = (ulong)properties[capacity];
				ViewModel.DriveFreeSpaceValue = (ulong)properties[freeSpace];
				ViewModel.DriveUsedSpaceValue = ViewModel.DriveCapacityValue - ViewModel.DriveFreeSpaceValue;
				ViewModel.DriveFileSystem = (string)properties[fileSystem];
			}
			catch (Exception e)
			{
				ViewModel.LastSeparatorVisibility = false;
				App.Logger.Warn(e, e.Message);
			}
		}
	}
}