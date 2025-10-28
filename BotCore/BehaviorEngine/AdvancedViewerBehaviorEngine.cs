using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace BotCore.BehaviorEngine
{
    public class AdvancedViewerBehaviorEngine
    {
        private readonly ILogger<AdvancedViewerBehaviorEngine> _logger;
        private IPage _page;
        private List<Action> _platformBehaviors = new List<Action>();

        public AdvancedViewerBehaviorEngine(ILogger<AdvancedViewerBehaviorEngine> logger)
        {
            _logger = logger;
        }

        public void SetBrowserPage(IPage page)
        {
            _page = page;
        }

        public void SetPlatformBehaviors(List<Action> behaviors)
        {
            _platformBehaviors = behaviors;
        }
    }
}

