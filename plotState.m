clear; close all;

listing = dir('*.csv');

for (i = 1:length(listing))
   M = csvread(listing(i).name, 1, 0);
%    save(strcat(listing(i).name(1:end-4), '.mat'), 'M')
%    clear M
end



plot(M(:,1), (M(:,4)-M(1,4)).*1000); title('Altitude (m)') 
figure()
plot(M(:,1), M(:,7).*1000); title('Velocity (m/s)')