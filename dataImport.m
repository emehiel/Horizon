clear all
close all
%Get Dynamic State Data
dataDir = '~/OneDrive/HorizonData/StateData/';
listing = dir(strcat(dataDir, '*.csv'));

for (i = 1:length(listing))
   M = csvread(strcat(dataDir, listing(i).name), 1, 0);
   save(strcat(listing(i).name(1:end-4), '.mat'), 'M')
   clear M
end

%Get State Data
dataDir = '~/OneDrive/HorizonData/output-*/';
dataDirs = dir(dataDir);
dataDir = strcat(dataDir(1:end-9), dataDirs.name, '/');
listing = dir(strcat(dataDir, '*.csv'));

for (i = 1:length(listing))
    if i ~=4  && i ~=12 
        M = csvread(strcat(dataDir, listing(i).name), 1, 0);
        save(strcat(listing(i).name(1:end-4), '.mat'), 'M')
        clear M
    end
end

%% Plot Data
listing = dir('*.mat');
names = {'Data Buffer Fill Ratio', 'Data Rate', 'Depth of Discharge',...
         'Position', 'Earth Observing Sensor On/Off', 'Incidence Angle', 'Number of Pixels', ...
        'Solar Panel Power In'};
units = ['%', 'MB/s',  '%', 'km','On/Off', 'Degrees', 'Pixels', 'Kw' ];
assetName = {'Asset 1 '};
assetCount = 0;
lineWidth = 1.5;
for(i = 1:length(listing))
   if (i > 8)
       assetName = {'Asset 2 '};
       assetCount = 8;
   end
   load(listing(i).name)
   figure(i)
   if(i == 4 || i == 12)
       stairs(M(:, 1), M(:, 2), 'LineWidth', lineWidth)
       stairs(M(:, 1), M(:, 3), 'r', 'LineWidth', lineWidth)
       stairs(M(:, 1), M(:, 4), 'k', 'LineWidth', lineWidth)
       legend('X Position', 'Y Position', 'Z Position')
   elseif i == 1 || i == 3 || i ==9 || i==11
       stairs(M(:, 1), M(:, 2)*100, 'LineWidth', lineWidth)
   else
       stairs(M(:, 1), M(:, 2), 'LineWidth', lineWidth)
   end
   title(strcat(assetName, ' ', names(i-assetCount)), 'FontSize', 20)
   set(gca, 'FontSize', 14)
   xlabel('Time', 'FontSize', 14)
   ylabel(units(i-assetCount), 'FontSize', 14)
   set(gcf, 'color', [1 1 1])
end
