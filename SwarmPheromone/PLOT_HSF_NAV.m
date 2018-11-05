function [] = PLOT_HSF_NAV(x1,y1,varargin)

switch nargin
    case 2
        plot( x1, y1 ); hold on
        
    case 4
        x2 = varargin{1};
        y2 = varargin{2};
        
        figure()
        plot( x1, y1 ); hold on
        plot( x2, y2 );
        
    case 6
        x2 = varargin{1};
        y2 = varargin{2};
        x3 = varargin{3};
        y3 = varargin{4};
        
        figure()
        plot( x1, y1 ); hold on
        plot( x2, y2 );
        plot( x3, y3 );
        
    case 8
        x2 = varargin{1};
        y2 = varargin{2};
        x3 = varargin{3};
        y3 = varargin{4};
        x4 = varargin{5};
        y4 = varargin{6};
        
        figure()
        plot( x1, y1 ); hold on
        plot( x2, y2 );
        plot( x3, y3 );
        plot( x4, y4 );
        
    otherwise
        error("Incorrect number of inputs")
        
end
        
plot(-120.603, 34.804, 'kp', 'markerfacecolor','red')
xlabel('Longitude (deg)')
ylabel('Latitude (deg)')