clear; close all;

listing = dir('*.csv');

for (i = 1:length(listing))
   M = csvread(listing(i).name, 1, 0);
%    save(strcat(listing(i).name(1:end-4), '.mat'), 'M')
%    clear M
end



plot(M(:,1), (M(:,2)-M(1,2)).*1000*3.2808); title('Altitude (ft)') 
figure()
plot(M(:,1), M(:,5).*1000); title('Velocity (m/s)')


figure()
plot((M(:,3)-M(1,3)).*1000*3.2808, (M(:,4)-M(1,4)).*1000*3.2808); title('Downrange Position (ft)') 